using MathNet.Numerics.LinearAlgebra.Double;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Terwiel.Glaucon
{


    public partial class Glaucon
    {
        /// <summary>
        /// Describes the load case.
        /// </summary>
        public partial class LoadCase
        {
            #region PublicFields

            /// <summary>
            /// TODO The max peak forces.
            /// </summary>
            public DenseMatrix MaxPeakForces;

            /// <summary>
            /// TODO The max peak displacements.
            /// </summary>
            public DenseMatrix MaxPeakDisplacements;

            /// <summary>
            /// TODO The min peak forces.
            /// </summary>
            public DenseMatrix MinPeakForces;

            /// <summary>
            /// TODO The min peak displacements.
            /// </summary>
            public DenseMatrix MinPeakDisplacements;

            #endregion

#if false // new peak forces
        public void GetPeakForces(Member[] members, DenseVector displacements)
        {
            double
                xx1,
                xx2 ;  // trapz load data, local z dir

            double
                tx = 0.0,
                tx_ = 0.0;  // distributed torque about local x Coord
            DenseVector
                w = new DenseVector(3),
                w_ = new DenseVector(3);
            double
                xp,  // location of internal point loads	
                Dx,
                x;   // distance along frame element	

            int m;   // frame element number
#if DEBUG
            long ticks = DateTime.Now.Ticks;
#endif
            int NrOfMembers = members.Length;
            MaxPeakForces = new List<DenseVector>(NrOfMembers );
            MinPeakForces = new List<DenseVector>(NrOfMembers);
            MaxPeakDisplacements = new List<DenseVector>(NrOfMembers);
            MinPeakDisplacements = new List<DenseVector>(NrOfMembers);
            for (var i = 0; i < NrOfMembers; i++)
            {
                MaxPeakForces.Add(new DenseVector(6));
                MaxPeakDisplacements.Add(new DenseVector(6));
                MaxPeakForces[i].MapInplace(s => Double.MinValue);
                MaxPeakDisplacements[i].MapInplace(s => Double.MinValue);
                MinPeakForces.Add(new DenseVector(6));
                MinPeakDisplacements.Add(new DenseVector(6));
                MinPeakForces[i].MapInplace(s => Double.MaxValue);
                MinPeakDisplacements[i].MapInplace(s => Double.MaxValue);
            }

            for (m = 0; m <NrOfMembers; m++)
            {   // loop over all frame elements

                var mbr = members[m];

                Dx = mbr.Length / Glaucon.Param.Xincr;// x-axis increment, same for each element
                // distributed gravity load in local x, y, z coordinates:
                var wg = (mbr.gamma.SubMatrix(0, 3, 0, 3) * g) * mbr.Material.Density * mbr.Area;

                // add uniformly-distributed loads to gravity load
                if (UniformLoads.Count > 0)
                    foreach (var u in UniformLoads)
                        if (u.MemberNr == m)
                            wg += u.q;

                // interior forces for frame element "m" at (x=0)
                var IntForces = -Q.Row(mbr.Nr).SubVector(0, 6);
                IntForces[4] *= -1;
                var IntForces_ = DenseVector.Build.Dense(6);
                IntForces.CopyTo(IntForces_);

                var locDispl = DenseVector.Build.Dense(6);

                // compute end deflections in local coordinates
                for (var i = 0; i < 6; i++)
                {
                    locDispl[i] = displacements[mbr.NodeA * 6 + i];// u
                    // locDispl[i+6] = displacements[mbr.NodeB * 6 + i];
                }


// rotations and displacements for frame element "m" at (x=0)
                locDispl = mbr.gamma.SubMatrix(0, 6, 0, 6) * locDispl;
                var temp = locDispl[5];
                locDispl[5] = locDispl[4];
                locDispl[4] = temp;
                var locDispl_ = DenseVector.Build.Dense(6);
                locDispl_[4] = locDispl[4];
                locDispl_[5] = locDispl[5];

                // accumulate interior span loads, forces, moments, slopes,
                // and displacements all in a single loop
                for (var i = 0; i < Glaucon.Param.Xincr; i++)
                {
                    x = i * Dx;
                    wg.CopyTo(w);

                    if (i == 1)
                    {
                        wg.CopyTo(w_);
                        tx_ = tx;
                    }
                    foreach (var trapLoads in TrapLoads)
                    {
                        if (trapLoads.MemberNr == m)
                        {
                            var lds = trapLoads.loads;
                            for(int j = 0; j < 3; j++)
                            {
                                var ld = lds[j];
                                xx1 = ld.a;


// xx2 = ld.b;
                                if (x > xx1 && x <= ld.b)
                                    w[j] += ld.Wa + (ld.Wb - ld.Wa) * (x - xx1) / (ld.Wb - ld.Wa);
                            }
                        }
                    }

                    // trapezoidal integration of distributed loads
                    // for axial forces, shear forces and torques
                    IntForces.SetSubVector(0, 3, IntForces.SubVector(0, 3) - (w + w_) * Dx / 2.0);
                    IntForces[3] -= 0.5 * (tx + tx_) * Dx;

                    // update distributed loads at x = (i-1)*dx
                    w.CopyTo(w_);
                    tx_ = tx;

                    // add interior point loads
                    foreach (var pl in IntPointLoads)
                    {
                        if (pl.MemberNr == m)
                        {
                            xp = pl.Position;
                            if (x <= xp && xp < x + Dx)
                            {
                                IntForces.SetSubVector(0, 3, IntForces.SubVector(0, 3) - pl.Load * 0.5 * (1.0 - (xp - x) / Dx));


// IntForces[0] -= pl.Load[0] * 0.5 * (1.0 - (xp - x) / Dx);
                                // IntForces[1] -= pl.Load[1] * 0.5 * (1.0 - (xp - x) / Dx);
                                // IntForces[2] -= pl.Load[2] * 0.5 * (1.0 - (xp - x) / Dx);
                            }
                            if (x - Dx <= xp && xp < x)
                            {
                                IntForces.SetSubVector(0, 3, IntForces.SubVector(0, 3) - pl.Load * 0.5 * (1.0 - (x - Dx - xp) / Dx));


// IntForces[0] -= pl.Load[0] * 0.5 * (1.0 - (x - Dx - xp) / Dx);
                                // IntForces[1] -= pl.Load[1] * 0.5 * (1.0 - (x - Dx - xp) / Dx);
                                // IntForces[2] -= pl.Load[2] * 0.5 * (1.0 - (x - Dx - xp) / Dx);
                            }
                        }
                    }

                    // trapezoidal integration of shear force for bending momemnt
                    IntForces[4] -= 0.5 * (IntForces_[2] + IntForces[2]) * Dx;
                    IntForces[5] -= 0.5 * (IntForces_[1] + IntForces[1]) * Dx;

                    // displacement along this member
                    locDispl[0] += 0.5 * (IntForces_[0] + IntForces[0]) / (mbr.Material.E * mbr.Area) * Dx;

                    // torsional rotation along frame element "m"
                    locDispl[3] += 0.5 * (IntForces_[3] + IntForces[3]) / (mbr.Material.G * mbr.Ipol) * Dx;

                    // transverse slope along this member
                    IntForces[4] += 0.5 * (IntForces_[5] + IntForces[5]) / (mbr.Material.E * mbr.IxZ) * Dx;
                    IntForces[5] += 0.5 * (IntForces_[4] + IntForces[4]) / (mbr.Material.E * mbr.IyZ) * Dx;

                    if (shear)
                    {
                        locDispl[4] += IntForces[1] / (mbr.Material.G * mbr.Asy);
                        locDispl[5] += IntForces[2] / (mbr.Material.G * mbr.Asz);
                    }

                    // displacement along this member
                    locDispl[1] += 0.5 * (locDispl_[4] + locDispl[4]) * Dx;
                    locDispl[2] += 0.5 * (locDispl_[5] + locDispl[5]) * Dx;

                    // remember forces and displacements for the next loop:
                    IntForces.CopyTo(IntForces_);
                    locDispl.CopyTo(locDispl_);

                    // update the peak forces, moments, slopes and displacements
                    // and their locations along the frame element
                    MaxPeakForces[mbr.Nr] = (DenseVector)MaxPeakForces[mbr.Nr].PointwiseMaximum(IntForces);
                    MaxPeakDisplacements[mbr.Nr] = (DenseVector)MaxPeakDisplacements[mbr.Nr].PointwiseMaximum(locDispl);

                    MinPeakForces[mbr.Nr] = (DenseVector)MinPeakForces[mbr.Nr].PointwiseMinimum(IntForces);
                    MinPeakDisplacements[mbr.Nr] = (DenseVector)MinPeakDisplacements[mbr.Nr].PointwiseMinimum(locDispl);

                }



#if DEBUG

#endif
            }// end of member loop
#if DEBUG
            ticks = DateTime.Now.Ticks - ticks;
            Debug.WriteLine($"Elapsed time for peaks computation loadcase {Nr+1}: {new TimeSpan(ticks).Milliseconds} msec.");
#endif
        }
#else // old

            /// <summary>
            /// Calculate the peak forces in all members.
            /// </summary>
            /// <Param name="members">an array of all members</Param>
            /// <Param name="displacements">Displacements of the member joints</Param>
            private void GetPeakForces(List<Member> members, DenseVector displacements)
            {
                Contract.Requires(members != null);
                Contract.Requires(displacements != null);

                // double xx1, xx2, wx1, wx2,  // trapz load data, local x dir .
                // xy1, xy2, wy1, wy2,  // trapz load data, local y dir .
                // xz1, xz2, wz1, wz2;  // trapz load data, local z dir .
                var w = new DenseVector(3);
                var w_ = new DenseVector(3);
                var wg = new DenseVector(3);
                double // wx = 0, wy = 0, wz = 0, // distributed loads in local coords at x[i]
                       // wx_ = 0, wy_ = 0, wz_ = 0,// distributed loads in local coords at x[i-1]
                       // wxg = 0, wyg = 0, wzg = 0,// gravity loads in local x, y, z Coord's
                    tx = 0.0, tx_ = 0.0; // distributed torque about local x Coord

               // int m; // frame element number
#if DEBUG
                var ticks = DateTime.Now.Ticks;
#endif


                this.MaxPeakForces = new DenseMatrix(members.Count, 6);
                this.MaxPeakDisplacements = new DenseMatrix(members.Count, 6);
                this.MinPeakForces = new DenseMatrix(members.Count, 6);
                this.MinPeakDisplacements = new DenseMatrix(members.Count, 6);

                var load = new DenseVector(6);
                var disp = new DenseVector(6);

                foreach (var mbr in members)
                {
                    double Nx, /* axial force within frame el.		*/
                           Vy,
                           Vz, /* shear forces within frame el.	*/
                           Tx, /* torsional moment within frame el.	*/
                           My,
                           Mz, /* bending moments within frame el.	*/
                           Sz,
                           Sy; /* transverse slopes of frame el.	*/


                    var n1 = mbr.NodeA.Nr;
                    var n2 = mbr.NodeB.Nr; // node 1 and node 2 of elmnt m

                    // number of sections along x axis
                    var nx = (int)Math.Round(
                        mbr.Length / Param.XIncrement,
                        0); // x-axis increment, same for each element
                    var dx = mbr.Length / nx;

                    // no need to allocate memory for interior force or displacement data

                    // find interior axial force, shear forces, torsion and bending moments
                    var gamma = mbr.Gamma.SubMatrix(0, 3, 0, 3);

                    // distributed gravity load in local x, y, z coordinates
                    (gamma * this.g * mbr.Mat.Density * mbr.As[0]).CopyTo(wg);

                    // add uniformly-distributed loads to gravity load
                    if (this.UniformLoads != null)
                    {
                        var loads = this.UniformLoads.Where(ul => ul.MemberNr == mbr.Nr);
                        foreach (var ul in loads)
                        {
                            wg.Add(ul.Q);
                        }
                    }

                    // interior forces for frame element "m" at (x=0)
                    var Nx_ = Nx = -this.Q[mbr.Nr, 0]; // positive Nx is tensile
                    var Vy_ = Vy = -this.Q[mbr.Nr, 1]; // positive Vy in local y direction
                    var Vz_ = Vz = -this.Q[mbr.Nr, 2]; // positive Vz in local z direction
                    var Tx_ = Tx = -this.Q[mbr.Nr, 3]; // positive Tx r.h.r. about local x axis
                    var My_ = My = this.Q[mbr.Nr, 4]; // positive My -> positive x-z curvature
                    var Mz_ = Mz = -this.Q[mbr.Nr, 5]; // positive Mz -> positive x-y curvature

                    var i1 = 6 * n1;

                    // compute end deflections in local coordinates
                    var u1 = gamma.Row(0) * displacements.SubVector(
                                 i1,
                                 3); // t1 * displacements[i1 + 1] + t2 * displacements[i1 + 2] + t3 * displacements[i1 + 3];
                    var u2 = gamma.Row(1) * displacements.SubVector(
                                 i1,
                                 3); // t4 * displacements[i1 + 1] + t5 * displacements[i1 + 2] + t6 * displacements[i1 + 3];
                    var u3 = gamma.Row(2) * displacements.SubVector(
                                 i1,
                                 3); // t7 * displacements[i1 + 1] + t8 * displacements[i1 + 2] + t9 * displacements[i1 + 3];

                    var u4 = gamma.Row(0) * displacements.SubVector(
                                 i1 + 3,
                                 3); // t1 * displacements[i1 + 4] + t2 * displacements[i1 + 5] + t3 * displacements[i1 + 6];
                    var u5 = gamma.Row(1) * displacements.SubVector(
                                 i1 + 3,
                                 3); // t4 * displacements[i1 + 4] + t5 * displacements[i1 + 5] + t6 * displacements[i1 + 6];
                    var u6 = gamma.Row(2) * displacements.SubVector(
                                 i1 + 3,
                                 3); // t7 * displacements[i1 + 4] + t8 * displacements[i1 + 5] + t9 * displacements[i1 + 6];

                    // rotations and displacements for frame element "m" at (x=0)
                    var Dx = u1; // displacement in  local x dir  at node NodeA
                    var dy = u2; // displacement in  local y dir  at node NodeA
                    var dz = u3; // displacement in  local z dir  at node NodeA
                    var Rx = u4; // rotation about local x axis at node NodeA
                    var Sy_ = Sy = u6; // slope in  local y  direction  at node NodeA
                    var Sz_ = Sz = -u5; // slope in  local z  direction  at node NodeA

                    // accumulate interior span loads, forces, moments, slopes, and displacements
                    // all in a single loop
                    for (var i = 1; i <= nx; i++)
                    {
                        var x = i * dx; // location from node NodeA along the x-axis

                        // start with gravitational plus uniform loads
                        wg.CopyTo(w);

                        if (i == 1)
                        {
                            wg.CopyTo(w_);
                            tx_ = tx;
                        }

                        // add trapezoidally-distributed loads
                        if (this.TrapLoads != null)
                        {
                            foreach (var tl in this.TrapLoads)
                            {
                                if (tl.MemberNr == mbr.Nr)
                                {
                                    // load n on element m
                                    // if (i == nx) ++cW;
                                    var xx1 = tl.Loads[0].a;
                                    var xx2 = tl.Loads[0].b;
                                    var wx1 = tl.Loads[0].Wa;
                                    var wx2 = tl.Loads[0].Wb;
                                    var xy1 = tl.Loads[1].a;
                                    var xy2 = tl.Loads[1].b;
                                    var wy1 = tl.Loads[1].Wa;
                                    var wy2 = tl.Loads[1].Wb;
                                    var xz1 = tl.Loads[2].a;
                                    var xz2 = tl.Loads[2].b;
                                    var wz1 = tl.Loads[2].Wa;
                                    var wz2 = tl.Loads[2].Wb;

                                    if (x > xx1 && x <= xx2)
                                    {
                                        w[0] += wx1 + (wx2 - wx1) * (x - xx1) / (xx2 - xx1);
                                    }

                                    if (x > xy1 && x <= xy2)
                                    {
                                        w[1] += wy1 + (wy2 - wy1) * (x - xy1) / (xy2 - xy1);
                                    }

                                    if (x > xz1 && x <= xz2)
                                    {
                                        w[2] += wz1 + (wz2 - wz1) * (x - xz1) / (xz2 - xz1);
                                    }
                                }
                            }
                        }

                        // trapezoidal integration of distributed loads
                        // for axial forces, shear forces and torques
                        Nx -= 0.5 * (w[0] + w_[0]) * dx;
                        Vy -= 0.5 * (w[1] + w_[1]) * dx;
                        Vz -= 0.5 * (w[2] + w_[2]) * dx;
                        Tx -= 0.5 * (tx + tx_) * dx;

                        // update distributed loads at x = (i-1)*dx
                        w.CopyTo(w_);

                        tx_ = tx;

                        // add interior point loads
                        if (this.IntPointLoads != null)
                        {
                            foreach (var pl in this.IntPointLoads)
                            {
                                if (pl.MemberNr == mbr.Nr)//??
                                {
                                    // load n on element m
                                    var xp = pl.Position;
                                    if (x <= xp && xp < x + dx)
                                    {
                                        Nx -= pl.Load[0] * 0.5 * (1.0 - (xp - x) / dx);
                                        Vy -= pl.Load[1] * 0.5 * (1.0 - (xp - x) / dx);
                                        Vz -= pl.Load[2] * 0.5 * (1.0 - (xp - x) / dx);
                                    }

                                    if (x - dx <= xp && xp < x)
                                    {
                                        Nx -= pl.Load[0] * 0.5 * (1.0 - (x - dx - xp) / dx);
                                        Vy -= pl.Load[1] * 0.5 * (1.0 - (x - dx - xp) / dx);
                                        Vz -= pl.Load[2] * 0.5 * (1.0 - (x - dx - xp) / dx);
                                    }
                                }
                            }
                        }

                        // trapezoidal integration of shear force for bending moment
                        My -= 0.5 * (Vz_ + Vz) * dx;
                        Mz -= 0.5 * (Vy_ + Vy) * dx;

                        // displacement along frame element "m"
                        Dx += 0.5 * (Nx_ + Nx) / (mbr.Mat.E * mbr.As[0]) * dx;

                        // torsional rotation along frame element "m"
                        Rx += 0.5 * (Tx_ + Tx) / (mbr.Mat.G * mbr.Iz[2]) * dx;

                        // transverse slope along frame element "m"
                        Sy += 0.5 * (Mz_ + Mz) / (mbr.Mat.E * mbr.Iz[0]) * dx;
                        Sz += 0.5 * (My_ + My) / (mbr.Mat.E * mbr.Iz[1]) * dx;

                        if (Param.AccountForShear)
                        {
                            Sy += Vy / (mbr.Mat.G * mbr.As[1]);
                            Sz += Vz / (mbr.Mat.G * mbr.As[2]);
                        }

                        // displacement along frame element "m"
                        dy += 0.5 * (Sy_ + Sy) * dx;
                        dz += 0.5 * (Sz_ + Sz) * dx;

                        // update forces, moments, and slopes at x = (i-1)*dx
                        Nx_ = Nx;
                        Vy_ = Vy;
                        Vz_ = Vz;
                        Tx_ = Tx;
                        My_ = My;
                        Mz_ = Mz;
                        Sy_ = Sy;
                        Sz_ = Sz;

                        load.SetValues(new[] { Nx, Vy, Vz, Tx, My, Mz });
                        load.PointwiseAbs();
                        disp.SetValues(new[] { Dx, dy, dz, Rx, Sy, Sz });
                        disp.PointwiseAbs();

                        // update the peak forces, moments, slopes and displacements
                        // and their locations along the frame element
                        this.MaxPeakForces.SetRow(mbr.Nr, (DenseVector)this.MaxPeakForces.Row(mbr.Nr).PointwiseMaximum(load));
                        this.MaxPeakDisplacements.SetRow(
                            mbr.Nr,
                            (DenseVector)this.MaxPeakDisplacements.Row(mbr.Nr).PointwiseMaximum(disp));
                        this.MinPeakForces.SetRow(mbr.Nr, (DenseVector)this.MinPeakForces.Row(mbr.Nr).PointwiseMinimum(load));
                        this.MinPeakDisplacements.SetRow(
                            mbr.Nr,
                            (DenseVector)this.MinPeakDisplacements.Row(mbr.Nr).PointwiseMinimum(disp));
                    }

                    // end of long loop along element "m"
                }

#if DEBUG
                ticks = DateTime.Now.Ticks - ticks;
                Debug.WriteLine(
                    $"Elapsed time for peaks computation loadcase {this.Nr + 1}: {new TimeSpan(ticks).Milliseconds} msec.");
#endif

                //WriteMatrix($"LC_{this.Nr + 1}_", "MinPeakForces.mat", "pfMin", this.MinPeakForces);
                //WriteMatrix($"LC_{this.Nr + 1}_", "MinPeakDispl.mat", "pdMin", this.MinPeakDisplacements);

                //WriteMatrix($"LC_{this.Nr + 1}_", "MaxPeakForces.mat", "pfMax", this.MaxPeakForces);
                //WriteMatrix($"LC_{this.Nr + 1}_", "MaxPeakDispl.mat", "pdMax", this.MaxPeakDisplacements);
            }

#endif // if old or new
        }
    }
}