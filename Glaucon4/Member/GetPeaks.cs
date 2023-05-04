using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Linq;

namespace Terwiel.Glaucon
{


    // using static Math;
    public partial class Glaucon
    {
        /// <summary>
        /// Internal class for construction members
        /// </summary>
        public partial class Member
        {
            public DenseVector maxPeakDisplacements = new(6);
            public DenseVector maxPeakForces = new(6);
            public DenseVector minPeakDisplacements = new(6);
            public DenseVector minPeakForces = new(6);

            public void GetMemberPeaks(LoadCase lc, DenseVector displacements)
            {
                if (lc == null || displacements == null)
                {
                    return;
                }

                double tx = 0.0, tx_ = 0.0;

                double Nx, /* axial force within frame el.		*/
                       Vy,
                       Vz, /* shear forces within frame el.	*/
                       Tx, /* torsional moment within frame el.	*/
                       My,
                       Mz, /* bending moments within frame el.	*/
                       Sz,
                       Sy; /* transverse slopes of frame el.	*/

                var w = new DenseVector(3);
                var w_ = new DenseVector(3);
                // uniform load by gravity:
                var wg = new DenseVector(3);

                var load = new DenseVector(6);
                var disp = new DenseVector(6);

                var n1 = NodeA.Nr;

                // number of sections along x axis
                var nx = (int)Math.Round(Length / Param.XIncrement, 0); // x-axis increment, same for each element
                var dx = Length / nx;

                // no need to allocate memory for interior force or displacement data

                // find interior axial force, shear forces, torsion and bending moments
                var gamma = this.Gamma.SubMatrix(0, 3, 0, 3);

                // distributed gravity load in local x, y, z coordinates
                (gamma * lc.g * Mat.Density * As[0]).CopyTo(wg);

                // add uniformly-distributed loads to gravity load
                if (lc.UniformLoads != null)
                {
                    wg = lc.UniformLoads.Where(ul => ul.MemberNr == Nr).Aggregate(wg, (current, ul) => current + ul.Q);
                }

                // interior forces for frame element "m" at (x=0)
                var Nx_ = Nx = -lc.Q[Nr, 0]; // positive Nx is tensile
                var Vy_ = Vy = -lc.Q[Nr, 1]; // positive Vy in local y direction
                var Vz_ = Vz = -lc.Q[Nr, 2]; // positive Vz in local z direction
                var Tx_ = Tx = -lc.Q[Nr, 3]; // positive Tx r.h.r. about local x axis
                var My_ = My = lc.Q[Nr, 4]; // positive My -> positive x-z curvature
                var Mz_ = Mz = -lc.Q[Nr, 5]; // positive Mz -> positive x-y curvature

                var i1 = 6 * n1;

                // compute end deflections in local coordinates
                var u1 = gamma.Row(0) * displacements.SubVector(i1, 3);
                var u2 = gamma.Row(1) * displacements.SubVector(i1, 3);
                var u3 = gamma.Row(2) * displacements.SubVector(i1, 3);

                var u4 = gamma.Row(0) * displacements.SubVector(i1 + 3, 3);
                var u5 = gamma.Row(1) * displacements.SubVector(i1 + 3, 3);
                var u6 = gamma.Row(2) * displacements.SubVector(i1 + 3, 3);

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

                    // add trapezoidal distributed loads
                    if (lc.TrapLoads != null)
                    {
                        foreach (var tl in lc.TrapLoads)
                        {
                            if (tl.MemberNr == Nr)
                            {
                                for (var j = 0; j < 3; j++)
                                {
                                    var tLoad = tl.Loads[j];
                                    if (x > tl.Loads[0].a && x <= tl.Loads[0].b)
                                    {
                                        w[j] += tLoad.Wa
                                                + ((tLoad.Wb - tLoad.Wa) * (x - tLoad.a)
                                                    / (tLoad.b - tLoad.a));
                                    }
                                }
                            }
                        }
                    }

                    // trapezoidal integration of distributed loads
                    // for axial forces, shear forces and torques
                    Nx -= (0.5 * (w[0] + w_[0]) * dx);
                    Vy -= (0.5 * (w[1] + w_[1]) * dx);
                    Vz -= (0.5 * (w[2] + w_[2]) * dx);
                    Tx -= (0.5 * (tx + tx_) * dx);

                    // update distributed loads at x = (i-1)*dx
                    w.CopyTo(w_);

                    tx_ = tx;

                    // add interior point loads
                    if (lc.IntPointLoads != null)
                    {
                        foreach (var pl in lc.IntPointLoads)
                        {
                            if (pl.MemberNr == Nr)
                            {
                                // load n on element m
                                var xp = pl.Position;
                                if (x <= xp && xp < x + dx)
                                {
                                    Nx -= pl.Load[0] * 0.5 * (1.0 - ((xp - x) / dx));
                                    Vy -= pl.Load[1] * 0.5 * (1.0 - ((xp - x) / dx));
                                    Vz -= pl.Load[2] * 0.5 * (1.0 - ((xp - x) / dx));
                                }

                                if (x - dx <= xp && xp < x)
                                {
                                    Nx -= pl.Load[0] * 0.5 * (1.0 - ((x - dx - xp) / dx));
                                    Vy -= pl.Load[1] * 0.5 * (1.0 - ((x - dx - xp) / dx));
                                    Vz -= pl.Load[2] * 0.5 * (1.0 - ((x - dx - xp) / dx));
                                }
                            }
                        }
                    }

                    // trapezoidal integration of shear force for bending moment
                    My -= (0.5 * (Vz_ + Vz) * dx);
                    Mz -= (0.5 * (Vy_ + Vy) * dx);

                    // displacement along frame element "m"
                    Dx += (((0.5 * (Nx_ + Nx)) / (Mat.E * As[0])) * dx);

                    // torsional rotation along frame element "m"
                    Rx += (((0.5 * (Tx_ + Tx)) / (Mat.G * Iz[2])) * dx);

                    // transverse slope along frame element "m"
                    Sy += (((0.5 * (Mz_ + Mz)) / (Mat.E * Iz[0])) * dx);
                    Sz += (((0.5 * (My_ + My)) / (Mat.E * Iz[1])) * dx);

                    if (Param.AccountForShear)
                    {
                        Sy += Vy / (Mat.G * As[1]);
                        Sz += Vz / (Mat.G * As[2]);
                    }

                    // displacement along frame element "m"
                    dy += (0.5 * (Sy_ + Sy) * dx);
                    dz += (0.5 * (Sz_ + Sz) * dx);

                    // update forces, moments, and slopes at x = (i-1)*dx
                    Nx_ = Nx;
                    Vy_ = Vy;
                    Vz_ = Vz;
                    Tx_ = Tx;
                    My_ = My;
                    Mz_ = Mz;
                    Sy_ = Sy;
                    Sz_ = Sz;

                    // TODO: don't use load and disp
                    load.SetValues(new[] { Nx, Vy, Vz, Tx, My, Mz, });
                    load.PointwiseAbs();
                    disp.SetValues(new[] { Dx, dy, dz, Rx, Sy, Sz, });
                    disp.PointwiseAbs();

                    // update the peak forces, moments, slopes and displacements
                    // and their locations along the frame element
                    maxPeakForces = (DenseVector)maxPeakForces.PointwiseMaximum(load);
                    maxPeakDisplacements = (DenseVector)maxPeakDisplacements.PointwiseMaximum(disp);
                    minPeakForces = (DenseVector)minPeakForces.PointwiseMinimum(load);
                    minPeakDisplacements = (DenseVector)minPeakDisplacements.PointwiseMinimum(disp);

                    WriteVector(LOADCASE,$"LC_{lc.Nr + 1}_", "minPeakForces.mat", "pfmin", minPeakForces);
                    WriteVector(LOADCASE,$"LC_{lc.Nr + 1}_", "minPeakDispl.mat", "pdmin", minPeakDisplacements);

                    WriteVector(LOADCASE, $"LC_{lc.Nr + 1}_", "maxPeakForces.mat", "pfmax", maxPeakForces);
                    WriteVector(LOADCASE, $"LC_{lc.Nr + 1}_", "maxPeakDispl.mat", "pdmax", maxPeakDisplacements);
                }
            }
        }
    }
}