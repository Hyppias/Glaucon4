#region FileHeader
// Project: Glaucon4
// Filename:   CubicBentBeam.cs
// Last write: 4/30/2023 2:29:46 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        public partial class Member
        {
            /// <summary>
            /// *CubicBentBeam  -  computes cubic deflection functions from end deflections
            /// and end rotations.Saves deflected coordinates to a plot file.These bent shapes
            /// are exact for mode-shapes, and for frames loaded at their nodes.
            /// </summary>
            /// <param name="defrm">The file to write to</param>
            /// <param name="D_">Member end displacements in global coordinates</param>
            /// <param name="exagg">deformation exaggeration</param>
            public void CubicBentBeam(StreamWriter defrm, DenseVector D_, double exagg)
            {
                var D = new DenseVector(12);

                /* compute end deflections in local coordinates */
                for (var i = 0; i < 12; i += 6)
                {
                    var _k = (i == 0 ? NodeA.Nr : NodeB.Nr) * 6;
                    for (var j = 0; j < 6; j++)
                    {
                        D[i + j] = D_[_k + j];
                    }
                }

                // end deflections in local coordinates
                var u = Gamma * D * exagg;

                /* curve-fitting problem for a cubic polynomial */
                var a =
                    (DenseVector)DenseVector.Build.DenseOfArray(new[] { u[1], u[7], u[5], u[11] });
                var
                    b = (DenseVector)DenseVector.Build.DenseOfArray(new[] { u[2], u[8], u[4], u[10] });
                //a[0] = u[1]; b[0] = u[2];
                //a[1] = u[7]; b[1] = u[8];
                //a[2] = u[5]; b[2] = -u[4];
                //a[3] = u[11]; b[3] = -u[10];

                u[6] += Length;
                var A = (DenseMatrix)DenseMatrix.Build.DenseOfArray(
                    new[,]
                    {
                    {1, u[0], Sq(u[0]), Sq(u[0]) * u[0]},
                    {1, u[6], Sq(u[6]), Sq(u[6]) * u[6]},
                    {0, 1, 2d * u[0], 3d * Sq(u[0])},
                    {0, 1, 2d * u[6], 3d * Sq(u[6])}
                    });
                //A[0, 0] = 1d; A[0, 1] = u[0]; A[0, 2] = Sq(u[0]); A[0, 3] = Sq(u[0]) * u[0];
                //A[1, 0] = 1d; A[1, 1] = u[6]; A[1, 2] = Sq(u[6]); A[1, 3] = Sq(u[6]) * u[6];
                //A[2, 0] = 0d; A[2, 1] = 1d; A[2, 2] = 2d * u[0]; A[2, 3] = 3d * Sq(u[0]);
                //A[3, 0] = 0d; A[3, 1] = 1d; A[3, 2] = 2d * u[6]; A[3, 3] = 3d * Sq(u[6]);
                u[6] -= Length;

                //a = (DenseVector)A.LU().Solve(a);  // works!
                //b = (DenseVector) A.LU().Solve(b);
                lu_dcmp(A, a, true, true); // solve for cubic coef's
                lu_dcmp(A, b, false, true); // solve for cubic coef's

                var _s = new DenseVector(3);
                for (var s = u[0];
                    Math.Abs(s) <= 1.01d * Math.Abs(Length + u[6]);
                    s += Math.Abs(Length + u[6] - u[0]) / 10.0)
                {
                    _s[0] = s;
                    // deformed shape in local coordinates
                    _s[1] = a[0] + a[1] * s + a[2] * s * s + a[3] * s * s * s;
                    _s[2] = b[0] + b[1] * s + b[2] * s * s + b[3] * s * s * s;

                    /* deformed shape in global coordinates */
                    var d = (DenseMatrix)Gamma.SubMatrix(0, 3, 0, 3).Transpose() * _s;
                    for (var i = 0; i < 3; i++)
                    {
                        d[i] += NodeA.Coord[i];
                    }

                    defrm.WriteLine($"{d[0]:F3} {d[1]:F3} {d[2]:F3}");
                    //script.WriteLine( " %12.4e %12.4e %12.4e\n",
                    //	xyz[n1].x + dX, xyz[n1].y + dY, xyz[n1].z + dZ);
                }
            }
        }
    }
}
