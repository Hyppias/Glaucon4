#region FileHeader
// Project: Glaucon4
// Filename:   coordtrans.cs
// Last write: 4/30/2023 2:29:45 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;

using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        public partial class Member
        {
            private const bool Zvert = true; // the global Z axis is vertical or not

            /// <summary>
            /// Set up the coordinate transformation matrix Γ(12 x 12)
            /// </summary>
            /// <param name="coordA">coordinates of node A</param>
            /// <param name="coordB">coordinates of node B</param>
            private void SetupGamma(DenseVector coordA, DenseVector coordB)
            {
                double[,] lambda0;
                double den; // cosine and sine of Roll angle 

                var Cx = (coordB[0] - coordA[0]) / Length;
                var Cy = (coordB[1] - coordA[1]) / Length;
                var Cz = (coordB[2] - coordA[2]) / Length;

                var Cp = Math.Cos(Roll);
                var Sp = Math.Sin(Roll);
                // Zvert=1(true) : Z axis is vertical... rotate about Y-axis, then rotate about Z-axis
                // Zvert=0(false): Y axis is vertical... rotate about Z-axis, then rotate about Y-axis
                if (Zvert)
                {
                    if (Math.Abs(Cz) == 1.0)
                    {
                        lambda0 = new[,]
                        {
                        {0d, 0, Cz},
                        {-Cz * Sp, Cp, 0},
                        {-Cz * Cp, -Sp, 0}
                    };
                    }
                    else
                    {
                        den = Math.Sqrt(1.0 - Cz * Cz);
                        lambda0 = new[,]
                        {
                        {Cx, Cy, Cz},
                        {(-Cx * Cz * Sp - Cy * Cp) / den, (-Cy * Cz * Sp + Cx * Cp) / den, Sp * den},
                        {(-Cx * Cz * Cp + Cy * Sp) / den, (-Cy * Cz * Cp - Cx * Sp) / den, Cp * den}
                    };
                    }
                }
                else // the global Y axis is vertical
                {
                    if (Math.Abs(Cy) == 1.0)
                    {
                        lambda0 = new[,]
                        {
                        {0, Cy, 0},
                        {-Cy * Cp, 0, Sp},
                        {Cy * Sp, 0, Cp}
                    };
                    }
                    else
                    {
                        den = Math.Sqrt(1.0 - Cy * Cy);
                        lambda0 = new[,]
                        {
                        {Cx, Cy, Cz},
                        {(-Cx * Cy * Cp - Cz * Sp) / den, den * Cp, (-Cy * Cz * Cp + Cx * Sp) / den},
                        {(Cx * Cy * Sp - Cz * Cp) / den, -den * Sp, (Cy * Cz * Sp + Cx * Cp) / den}
                    };
                    }
                }

                // four times aong diagonal:
                for (var i = 0; i < 12; i += 3)
                {
                    Gamma.SetSubMatrix(i, i, DenseMatrix.Build.DenseOfArray(lambda0));
                }

                WriteMatrix(GAMMA, $"gamma_mbr{Nr}_", $"gamma_mbr{Nr}_", $"gamma{Nr}",
                    (DenseMatrix)Gamma.SubMatrix(0, 3, 0, 3));
            }
        }
    }
}
