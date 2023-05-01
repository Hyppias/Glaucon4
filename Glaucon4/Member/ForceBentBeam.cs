#region FileHeader
// Project: Glaucon4
// Filename:   ForceBentBeam.cs
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
            /// ForceBentBeam  -  reads internal frame element forces and deflections
            /// from the internal force and deflection matrices.
            /// Saves deflected shapes to a plot file.These bent shapes are exact.
            /// Note: It would not be difficult to adapt this function to plot
            /// internal axial force, Shear force, torques, or bending moments.
            /// </summary>
            /// <param name="defrm">THe GNUPlot script to write to</param>
            /// <param name="transvDispl">Transversal displacements of the nodes</param>
            /// <param name="exagg">Display exaggeration</param>
            public void ForceBentBeam(StreamWriter defrm, DenseMatrix transvDispl, double exagg)
            {
                var L = new DenseVector(3);

                // three length projections
                for (var i = 0; i < 3; i++)
                {
                    L[i] = NodeB.Coord[i] - NodeA.Coord[i];
                }

                // only pick 10 segments of a member:
                for (double xi = 0; xi <= 1.01 * Length; xi += 0.10 * Length)
                {
                    var n = (int)Math.Floor(xi / IncrPeak);

                    var D = (DenseVector)transvDispl.Row(n).SubVector(0, 3); // X, Y and Z

                    /* exaggerated deformed shape in global coordinates */
                    var d = (DenseMatrix)Gamma.SubMatrix(0, 3, 0, 3).Transpose() * D * exagg;

                    for (var i = 0; i < 3; i++)
                    {
                        defrm.Write($"{NodeA.Coord[i] + x[n] / Length * L[i] + d[i]:F3} ");
                    }

                    defrm.Write("\n");
                }
            }
        }
    }
}
