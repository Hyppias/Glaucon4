#region FileHeader
// Project: Glaucon4
// Filename:   SystemMatrices.cs
// Last write: 4/30/2023 2:29:41 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        public static DenseMatrix
            K, // System stiffness matrix
            M; // System Mass matrix

        public static void CheckEmptyRow(string name, DenseMatrix k)
        {
            for (var i = 0; i < k.RowCount; i++)
            {
                var m = 0;
                for (var j = 0; j < k.ColumnCount; j++)
                {
                    if (k[i, j] != 0)
                    {
                        m = 1;
                        break;
                    }
                }

                if (m == 0)
                {
                    Debug.WriteLine($"{name}[{i}] is empty");
                }
            }

            //Debug.WriteLine(name);
            //for (var i = 0; i < k.RowCount; i++)
            //{
            //    for (var j = 0; j < k.ColumnCount; j++)
            //    {
            //        Debug.Write(k[i, j] == 0 ? "0 " : "X ");
            //    }

            //    Debug.Write("\n");
            //}
        }

        /// <summary>
        /// assemble global stiffness matrix from individual elements
        /// Original: void geometric_K
        /// </summary>
        /// <param name="members">list of members</param>
        /// <param name="rearrange">Rearrange the matrix?</param>
        /// <param name="Q">List of compressive forces for each member</param>
        public static void AssembleSystemStiffnessMatrix(List<Member> members, bool rearrange = ReArrange, DenseMatrix Q = null, bool geom = false)
        {
            //StartClock(4);
            K.Clear();

            foreach (var mbr in members)
            {
                mbr.Elastic_K();
                if (geom && Q != null)
                // Q = axial force, eg. due to temperature change
                // Przemieniecki chapter 15, page 384, (15.1)
                {
                    mbr.GeometricMemberStiffnessMatrix(-Q[mbr.Nr,0 ]);
                }

                mbr.AddLocalToGlobal(K, mbr.k);
            }

            //CheckEmptyRow("K_orig",K);
            WriteMatrix(true, "exH", "K_exH", "K", K);
            // Now this system matrix is singular!
            // we must rearrange the rows and columns
            // and those of the force vector.
            if (rearrange)
            {
                // McGuire, pg. 40-1
                K.PermuteColumns(Perm);
                K.PermuteRows(Perm);
            }
            //CheckEmptyRow("K_rearr",K);
            //StopClock(4,"Building system stiffness matrix");
        }

        /// <summary>
        /// assemble global mass matrix M from member mass & inertia
        /// only used when calculating eigenfrequencies
        /// </summary>
        private void AssembleSystemMassMatrix()
        {
            M.Clear();
            foreach (var mbr in Members)
            {
                mbr.AddLocalToGlobal(M, Param.LumpedMassMatrix ? mbr.LumpedMassMatrix() : mbr.ConsistentMassMatrix());
            }

            foreach (var node in Nodes) //for (int j = 0; j < nN; j++)// add extra node mass
            {
                if (node.ExtraNodalMass != null)
                {
                    node.AddMassToMassMatrix(); //  may be zero...
                }
            }

            var i = M.Diagonal().MinimumIndex();
            if (M[i, i] <= 0.0)
            {
                throw new Exception($"error: Non pos-def mass matrix: M[{i},{i}] = {M[i, i]}");
            }
        }
    }
}
