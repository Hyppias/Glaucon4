#region FileHeader
// Project: Glaucon4
// Filename:   MbrPeakForces.cs
// Last write: 4/30/2023 2:29:48 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using dbl = MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    internal enum MM
    {
        Max = 0, // line 0
        Min = 1 // line 1
    }

    public partial class Glaucon
    {
        private const int
            Dx = 0,
            Dy = 1,
            Dz = 2,
            Rx = 3,
            Sy = 4,
            Sz = 5,
            Nx = 0,
            Vy = 1,
            Vz = 2,
            Tx = 3,
            My = 4,
            Mz = 5,
            X = 0,
            Y = 1,
            Z = 2;

        public partial class Member
        {
            dbl.DenseMatrix MinMaxForce;
            dbl.DenseMatrix MinMaxDispl;

            //// Finding the max/min forces/displacements must be
            //// done with a fine enough resolution (nr. of steps)          
            //[XmlMatrix("MinMax"), Description("Minimum and Maximum internal forces and displacements")]
            //public dbl.DenseMatrix[] MinMaxList { get; set; }

            private double DxLast;
            private int SegmentCnt; // was nx
            private dbl.DenseVector x;

            public void SetupMinMaxCalculation(LoadCase lc)
            {
                SegmentCnt = Math.Min(Param.MaxSegmentCount, Math.Max((int)(Length / deltaX / 10d) * 10, 10));
                Debug.Assert(SegmentCnt > 1, $"Member {Nr} segments too small");
                IncrPeak = Length / SegmentCnt; //overrule input file
                x = new dbl.DenseVector(SegmentCnt + 1);
                for (var i = 0; i < SegmentCnt; i++)
                {
                    x[i] = i * IncrPeak;
                }

                x[SegmentCnt] = Length;
                DxLast = x[SegmentCnt] -
                    x[SegmentCnt - 1]; // length of the last x-axis increment                             

                lc.IntForces[Nr] =  new dbl.DenseMatrix(SegmentCnt + 1, 6);
                lc.TransvDispl[Nr] = new dbl.DenseMatrix(SegmentCnt + 1, 6);
            }

            public void WritePeaks(LoadCase loadcase)
            {
                Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name + $": Member {Nr}");

                var g = loadcase.g;
                var Unif = loadcase.UniformLoads;
                var Q = loadcase.Q.Row(Nr); // copies the row to local Q
                var D = (dbl.DenseVector)loadcase.Displacements.Column(loadcase.Nr);
                var TrapLoads = loadcase.TrapLoads;
                var IntPointLoads = loadcase.IntPointLoads;
               // var MinMaxForce = loadcase.Maxdbl.DenseMatrix(members.Count,12);
                //var MinMaxDispl = loadcase.MinMaxDispl;
                var IntForces = loadcase.IntForces[Nr];
                var TransvDispl = loadcase.TransvDispl[Nr];

                double tx_ = 0.0, tx = 0.0;
                dbl.DenseVector
                    w = new dbl.DenseVector(3),
                    w_ = new dbl.DenseVector(3);

                try
                {
                    //Debug.WriteLine($"Load case {lc+1} member {Nr+1}");
                    //Debug.WriteLine($"Q0={Q[0]} Q1={Q[1]} Q2={Q[2] }Q3={Q[3]}Q4={Q[4]}Q5={Q[5]}");
                    //TODO: is this OK?
                    var wg =
                        (dbl.DenseVector)( Gamma.SubMatrix(0, 3, 0, 3).Multiply(g)).Multiply(Mat.Density * As[0]);

                    // add uniformly-distributed loads to gravity load
                    if (Unif?.Count > 0)
                    {
                        foreach (var unif in Unif)
                        {
                            if (unif.MemberNr == Nr)
                            {
                                for (var i = 0; i < 3; i++)
                                {
                                    wg[i] += unif.Q[i]; // q are Singles
                                }
                            }
                        }
                    }

                    // interior FORCES for this frame element at (x=0, NodeA)

                    for (var i = 0; i < 6; i++)
                    {
                        IntForces[0, i] = -Q[i];
                    }

                    IntForces[0, My] *= -1;
                    var dx_ = IncrPeak;
                    // accumulate interior span loads, forces, moments, slopes,
                    //  and displacements all in a single loop
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        var pos = x[i];
                        wg.CopyTo(w);

                        if (i == 1)
                        {
                            wg.CopyTo(w_);
                            tx_ = tx;
                        }

                        if (TrapLoads?.Count > 0)
                        {
                            foreach (var trapLoads in TrapLoads)
                            {
                                if (trapLoads.MemberNr == Nr)
                                {
                                    var lds = trapLoads.Loads;
                                    for (var j = 0; j < 3; j++)
                                    {
                                        var ld = lds[j];
                                        //xx1 = ld.a;
                                        if (pos > ld.a && pos <= ld.b)
                                        {
                                            w[j] += ld.Wa + (ld.Wb - ld.Wa) * (pos - ld.a) / (ld.b - ld.a);
                                        }
                                    }
                                }
                            }
                        }

                        // trapezoidal integration of distributed loads
                        // for axial forces, Shear forces and torques
                        if (i == SegmentCnt)
                        {
                            dx_ = DxLast;
                        }

                        for (var j = 0; j < 3; j++)
                        {
                            IntForces[i, j] = IntForces[i - 1, j] - 0.5 * (w[j] + w_[j]) * dx_;
                        }

                        IntForces[i, Tx] = IntForces[i - 1, Tx] - 0.5 * (tx + tx_) * dx_;

                        //Debug.WriteLine($"24 {i} Nx = {IntForces[i,0]} Vy = {IntForces[i, 1]} Vz = {IntForces[i, 2]} Tx = {IntForces[i, 3]}");
                        // update distributed loads at x = (i-1).IncrPeak
                        w.CopyTo(w_);
                        tx_ = tx;

                        // add interior point loads
                        if (IntPointLoads?.Count > 0)
                        {
                            // double xp;
                            foreach (var pl in IntPointLoads)
                            {
                                if (pl.MemberNr == Nr)
                                {
                                    double xp = pl.Position;
                                    if (pos <= xp && xp < pos + IncrPeak)
                                    {
                                        for (var j = 0; j < 3; j++)
                                        {
                                            IntForces[i, j] -= pl.Load[j] * 0.5 * (1.0 - (xp - pos) / IncrPeak);
                                        }
                                    }

                                    if (pos - IncrPeak <= xp && xp < pos)
                                    {
                                        for (var j = 0; j < 3; j++)
                                        {
                                            IntForces[i, j] -=
                                                pl.Load[j] * 0.5 * (1.0 - (pos - IncrPeak - xp) / IncrPeak);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // linear correction of forces for bias in trapezoidal integration
                    for (var i = 1; i <= SegmentCnt; i++)
                        for (var j = 0; j < 4; j++)
                        {
                            IntForces[i, j] -= (IntForces[SegmentCnt, j] - Q[6 + j]) * i / SegmentCnt;
                        }

                    // trapezoidal integration of Shear force for bending momemnt
                    dx_ = IncrPeak;
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        if (i == SegmentCnt)
                        {
                            dx_ = DxLast;
                        }

                        IntForces[i, My] = IntForces[i - 1, My] - 0.5 * (IntForces[i, Vz] + IntForces[i - 1, Vz]) * dx_;
                        IntForces[i, Mz] = IntForces[i - 1, Mz] - 0.5 * (IntForces[i, Vy] + IntForces[i - 1, Vy]) * dx_;
                    }

                    // linear correction of moments for bias in trapezoidal integration
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        IntForces[i, My] -= (IntForces[SegmentCnt, My] + Q[10]) * i / SegmentCnt;
                        IntForces[i, Mz] -= (IntForces[SegmentCnt, Mz] - Q[11]) * i / SegmentCnt;
                    }
#if true
                    // ======================================
                    // and now the internal displacements:
                    // ======================================

                    // compute end deflections in local coordinates

                    var d = new dbl.DenseVector(12);
                    for (var i = 0; i < 12; i++)
                    {
                        d[i] = D[ind[i]];
                    }

                    var u = Gamma * d;

                    for (var i = 0; i < 6; i++)
                    {
                        TransvDispl[0, i] = u[i];
                    }

                    TransvDispl[0, Sy] = u[Sz];
                    TransvDispl[0, Sz] = -u[Y];

                    double EAs0 = Mat.E * As[0];
                    dx_ = IncrPeak;
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        if (i == SegmentCnt)
                        {
                            dx_ = DxLast;
                        }

                        TransvDispl[i, Dx] = TransvDispl[i - 1, Dx] +
                            0.5 * (IntForces[i - 1, Nx] + IntForces[i, Nx]) / EAs0 * dx_;
                    }

                    // linear correction of axial displacement for bias in trapezoidal integration
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        TransvDispl[i, Dx] -= (TransvDispl[SegmentCnt, Dx] - u[6]) * i / SegmentCnt;
                    }

                    // torsional rotation along this frame element
                    double GIx = Mat.G * Iz[X]; // Polar
                    double EIy = Mat.E * Iz[Y]; // Linear YY
                    double EIz = Mat.E * Iz[Z]; // Linear ZZ
                    dx_ = IncrPeak;
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        if (i == SegmentCnt)
                        {
                            dx_ = DxLast;
                        }

                        TransvDispl[i, Rx] = TransvDispl[i - 1, Rx] +
                            0.5 * (IntForces[i - 1, Rx] + IntForces[i, Rx]) / GIx * dx_;
                    }

                    // linear correction of torsional rot'n for bias in trapezoidal integration
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        TransvDispl[i, Rx] -= (TransvDispl[SegmentCnt, Rx] - u[9]) * i / SegmentCnt;
                    }

                    // transverse slope along frame element "m"
                    dx_ = IncrPeak;
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        if (i == SegmentCnt)
                        {
                            dx_ = DxLast;
                        }

                        TransvDispl[i, Sy] = TransvDispl[i - 1, Sy] +
                            0.5 * (IntForces[i - 1, Sz] + IntForces[i, Sz]) / EIz * dx_;
                        TransvDispl[i, Sz] = TransvDispl[i - 1, Sz] +
                            0.5 * (IntForces[i - 1, Sy] + IntForces[i, Sy]) / EIy * dx_;
                    }

                    // linear correction for bias in trapezoidal integration
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        TransvDispl[i, Sy] -= (TransvDispl[SegmentCnt, Sy] - u[11]) * i / SegmentCnt;
                        TransvDispl[i, Sz] -= (TransvDispl[SegmentCnt, Sz] + u[10]) * i / SegmentCnt;
                    }

                    if (Param.AccountForShear)
                    {
                        // add-in slope due to Shear deformation
                        double GAy = Mat.G * As[Y];
                        double GAz = Mat.G * As[Z];
                        for (var i = 0; i <= SegmentCnt; i++)
                        {
                            TransvDispl[i, Sy] += IntForces[i, Vy] / GAy;
                            TransvDispl[i, Sz] += IntForces[i, Vz] / GAz;
                        }
                    }

                    // displacement along this frame element
                    dx_ = IncrPeak;
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        if (i == SegmentCnt)
                        {
                            dx_ = DxLast;
                        }

                        TransvDispl[i, Dy] = TransvDispl[i - 1, Dy] +
                            0.5 * (TransvDispl[i - 1, Sy] + TransvDispl[i, Sy]) * dx_;
                        TransvDispl[i, Dz] = TransvDispl[i - 1, Dz] +
                            0.5 * (TransvDispl[i - 1, Sz] + TransvDispl[i, Sz]) * dx_;
                    }

                    // linear correction for bias in trapezoidal integration
                    for (var i = 1; i <= SegmentCnt; i++)
                    {
                        TransvDispl[i, Dy] -= (TransvDispl[SegmentCnt, Dy] - u[7]) * i / SegmentCnt;
                        TransvDispl[i, Dz] -= (TransvDispl[SegmentCnt, Dz] - u[8]) * i / SegmentCnt;
                    }

                    // WriteMatrix(MINMAX, $"lc_{loadcase.Nr+1}_mbr{Nr+1}", "TransvDispl.dat", "TransvDispl", (dbl.DenseMatrix) TransvDispl.SubMatrix(0,SegmentCnt,0,4));
#endif
                    MinMaxForce = new dbl.DenseMatrix(2,12);
                    MinMaxDispl = new dbl.DenseMatrix(2,12);

                    for (var i = 0; i < 6; i++)
                    {
                        MinMaxForce[(int)MM.Max, i] = IntForces.Column(i)[IntForces.Column(i).MaximumIndex()];
                        MinMaxForce[(int)MM.Min, i] = IntForces.Column(i)[IntForces.Column(i).MinimumIndex()];

                        MinMaxDispl[(int)MM.Max, i] = TransvDispl.Column(i)[TransvDispl.Column(i).MaximumIndex()];
                        MinMaxDispl[(int)MM.Min, i] = TransvDispl.Column(i)[TransvDispl.Column(i).MinimumIndex()];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw new Exception($"Error in Peak calculation load case {loadcase.Nr} member {Nr}", ex);
                }
                finally
                {
                    Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name + $": Member {Nr}");
                }
            }
        }
    }
}
