#region FileHeader
// Project: Glaucon4
// Filename:   TrapeziumLoad.cs
// Last write: 4/30/2023 2:29:44 PM
// Creation:   4/27/2023 7:47:47 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using MathNet.Numerics.LinearAlgebra.Double;
using System.ComponentModel.Design;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        public partial class LoadCase
        {
            /// <summary>
            /// The trapezoidal distributed load.
            /// On a member there can be one or more systems of trapezoidal loads.
            /// A system of trapezoidal loads consists of three component, each in
            /// one of the X, Y and Z directions.
            /// </summary>
            public class TrapLoad : ILoad
            {
                public TrapLoad(int mbr, Load[] ld, bool active = true)
                {
                    MemberNr = mbr - 1;
                    Loads = ld;
                    Active = active;
                }

                public bool Active = true;
                /// <summary>
                /// Three components (X, Y and Z) of the trapeziodal load.
                /// </summary>
                public Load[] Loads;

                /// <summary>
                /// The member nr that the load is on.
                /// </summary>
                public int MemberNr; // Member Nr. base 0

                public DenseVector GetLoadVector(Member mbr)
                {
                    var fixedEndForces = new DenseVector(12);

                    double R1o = 0, R2o = 0, f01 = 0, f02 = 0;
                  
                    var length = mbr.Length;

                    // x-axis trapezoidal loads (along the frame member length) 
                    Loads[0].Build1(length, ref f01, ref f02);

                    fixedEndForces[0] = f01;
                    fixedEndForces[6] = f02;

                    // y-axis trapezoidal loads (across the frame member length) 
                    Loads[1].Build2(length, ref R1o, ref R2o, ref f01, ref f02);

                    fixedEndForces[5] = -(4.0 * f01 + 2.0 * f02 + mbr.Ksy * (f01 - f02))
                                        / (length * length * (1.0 + mbr.Ksy));
                    fixedEndForces[11] = -(2.0 * f01 + 4.0 * f02 - mbr.Ksy * (f01 - f02))
                                         / (length * length * (1.0 + mbr.Ksy));

                    fixedEndForces[1] = R1o + fixedEndForces[5] / length + fixedEndForces[11] / length;
                    fixedEndForces[7] = R2o - fixedEndForces[5] / length - fixedEndForces[11] / length;

                    // z-axis trapezoidal loads (across the frame member length) 
                    Loads[2].Build2(length, ref R1o, ref R2o, ref f01, ref f02);

                    fixedEndForces[4] = (4.0 * f01 + 2.0 * f02 + mbr.Ksz * (f01 - f02))
                                        / (length * length * (1.0 + mbr.Ksz));
                    fixedEndForces[10] = (2.0 * f01 + 4.0 * f02 - mbr.Ksz * (f01 - f02))
                                         / (length * length * (1.0 + mbr.Ksz));

                    fixedEndForces[2] = R1o - fixedEndForces[4] / length - fixedEndForces[10] / length;
                    fixedEndForces[8] = R2o + fixedEndForces[4] / length + fixedEndForces[10] / length;

                    return fixedEndForces;
                }

                /// <summary>
                /// One trapezoidal load
                /// </summary>
                public class Load
                {
                    public Load((double, double, double, double) p)
                    {
                        (a, b, Wa, Wb) = p;
                    }
                    /// <summary>
                    /// start point of the load, somewhere
                    /// along the member and between the start- and
                    /// end joint.
                    /// </summary>
                    public double a;

                    /// <summary>
                    /// end point of the load.
                    /// </summary>
                    public double b;

                    /// <summary>
                    /// load intensity at start.
                    /// Unity is N/m, normally.
                    /// </summary>
                    public double Wa;

                    /// <summary>
                    /// load intensity at end
                    /// </summary>
                    public double Wb;


                    public void Build1(double length, ref double f01, ref double f02)
                    {
                        f01 = (3.0 * (Wa + Wb) * length * (b - a)
                                  - (2.0 * Wb + Wa) * b * b + (Wb - Wa) * b * a
                                                            + (2.0 * Wa + Wb) * a
                                                                              * a)
                              / (6.0 * length);
                        f02 = (-(2.0 * Wa + Wb) * a * a
                               + (2.0 * Wb + Wa) * b * b
                               - (Wb - Wa) * a * b) / (6.0 * length);
                    }

                    public void Build2(double length, ref double R1o, ref double R2o, ref double f01, ref double f02)
                    {
                        R1o = ((2.0 * Wa + Wb) * a * a
                               - (Wa + 2.0 * Wb) * b * b
                               + 3.0 * (Wa + Wb) * length * (b - a)
                               - (Wa - Wb) * a * b) / (6.0 * length);
                        R2o = ((Wa + 2.0 * Wb) * b * b + (Wa - Wb) * a * b
                               - (2.0 * Wa + Wb) * a * a) / (6.0 * length);

                        f01 =
                            (3.0 * (Wb + 4.0 * Wa) * a * a * a * a
                             - 3.0 * (Wa + 4.0 * Wb) * b * b * b * b
                             - 15.0 * (Wb + 3.0 * Wa) * length * a * a * a
                             + 15.0 * (Wa + 3.0 * Wb) * length * b * b * b
                             - 3.0 * (Wa - Wb) * a * b * (a * a + b * b)
                             + 20.0 * (Wb + 2.0 * Wa) * length * length * a * a
                             - 20.0 * (Wa + 2.0 * Wb) * length * length * b * b
                             + 15.0 * (Wa - Wb) * length * a * b * (a + b)
                             - 3.0 * (Wa - Wb) * a * a * b * b
                             - 20.0 * (Wa - Wb) * length * length * a * b) / 360.0;

                        f02 = (3.0 * (Wb + 4.0 * Wa) * a * a * a * a
                               - 3.0 * (Wa + 4.0 * Wb) * b * b * b * b
                               - 3.0 * (Wa - Wb) * a * b * (a * a + b * b)
                               - 10.0 * (Wb + 2.0 * Wa) * length * length * a * a
                               + 10.0 * (Wa + 2.0 * Wb) * length * length * b * b
                               - 3.0 * (Wa - Wb) * a * a * b * b
                               + 10.0 * (Wa - Wb) * length * length * a * b) / 360.0;
                    }
                }
            }
        }
    }
}
