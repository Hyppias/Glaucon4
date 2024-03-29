#region FileHeader
// Project: Glaucon4
// Filename:   Condensation.cs
// Last write: 4/30/2023 2:49:36 PM
// Creation:   4/24/2023 11:59:08 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.Diagnostics;
using System.Reflection;
using static Terwiel.Glaucon.Glaucon;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        // CondensationMethod (Cmethod): 1 = static, 2 = Guyan, 3 = Dynamic 
        private void ProcessCondensationData()
        {
            if (CondensedNodes.Count > Nodes.Count)
            {
                string message = $"number of nodes with DoF's to condense {CondensedNodes.Count} larger than number of nodes {Nodes.Count}";

                throw new ArgumentOutOfRangeException(message);
            }

            if (Param.CondensationMethod > 3)
            {
                Param.CondensationMethod = 1;
            }

            // array c in FRAME3DD
            DoFsToCondense = new int[DoF];
            // List was already set up, but DoF number are still base 1
            foreach (var cn in CondensedNodes) // List was already set up, but DoF number are still index 1
            {
                cn.NodeNr--; // adjust node numbering to base 0
                if (cn.NodeNr < 0 || cn.NodeNr > Nodes.Count - 1)
                {
                    string message = $"Node number ({cn.NodeNr}) out of range in condensation data.";
                    throw new IndexOutOfRangeException(message);
                }

                Cdof = 0; // for being sure
                // set up the array of DoFs to condense:
                for (var j = 0; j < 6; j++)
                {
                    if (cn.DoFs[j] != 0) // read 1's and 0's
                    {
                        // make a list of DoF's to condense (c[] in FRAME3DD)
                        DoFsToCondense[Cdof] = 6 * cn.NodeNr + j; ///cn.DoFToCondense[j];
                        Cdof++;
                    }
                }
            }

            //now, for each DoF to condense check the MODE number.

            //this is only for dynamic condensation!
            if (Param.CondensationMethod == Dynamic && Param.DynamicModesCount > Cdof)
            {
                throw new ArgumentOutOfRangeException(
                    $"The number of to be computed vibration modes ({Param.DynamicModesCount}) may\n" +
                    $" may not exceed the number of required modes of condensation ({Cdof}) when\n" +
                    "using dynamic condensation.");
            }

            // when using static condensation, only first mode is matched:
            for (int i = 0; i < (Param.CondensationMethod == 1 ? 1 : MatchedCondenseModes.Length); i++)
            {
                if (MatchedCondenseModes[i] < 0 || MatchedCondenseModes[i] < Cdof)
                {
                    throw new ArgumentOutOfRangeException($"Condensation mode {i + 1} out of range");
                }
            }
        }

        /// <summary>
        /// STATIC_CONDENSATION - of stiffness matrix from NxN to nxn
        /// The original vector c is DoF long, but only the lower elements
        /// have a DoF number, for which the DoF was set  to 1.
        /// CDof was the number of DoFs having a 1 entered, so the first part os array c.
        /// Original name: static_condensation
        /// </summary>
        /// <see cref="https://www.youtube.com/watch?v=E-KxCovY6OU"/>
        /// <seealso cref="McGuire et al. page 376"/>
        /// <seealso cref="Bathe, Finite element procedures, 
        /// PHI Learning Private Limited, Delhi 110092, 2015, page 720"/>
        public static void StaticCondensation()
        {
            var n = Cdof;
            Kc = new DenseMatrix(n);
            var Arr = new DenseMatrix(DoF - n);
            var Arc = new DenseMatrix(DoF - n, n);
            // gather all 1's from DoFToCondense in r
            var r = GenerateListOfDoFNotToCondense(DoF);

            for (var i = 0; i < DoF - n; i++)
            {
                for (var j = i; j < DoF - n; j++) /* use only upper triangle of K */
                {
                    var ri = r[i];
                    var rj = r[j];
                    if (ri <= rj)
                    {
                        Arr[j, i] = Arr[i, j] = K[ri, rj];
                    }
                }
            }
            for (var i = 0; i < DoF - n; i++)
            {
                for (var j = 0; j < n; j++) /* use only upper triangle of K */
                {
                    var ri = r[i];
                    var cj = DoFsToCondense[j];
                    if (ri < cj)
                    {
                        Arc[i, j] = K[ri, cj];
                    }
                    else
                    {
                        Arc[i, j] = K[cj, ri];
                    }
                }
            }
            xtinvAy(Arc, Arr, Arc, Kc); // uses symmetry
            // Kc = (DenseMatrix) Arc.Transpose().Inverse() * Arr * Arc;

            for (var i = 0; i < n; i++)
            {
                for (var j = i; j < n; j++) /* use only upper triangle of A */
                {
                    var ci = DoFsToCondense[i];
                    var cj = DoFsToCondense[j];
                    if (ci <= cj)
                    {
                        Kc[j, i] = Kc[i, j] = K[ci, cj] - Kc[i, j];
                    }
                }
            }
        }


        /// <summary>
        /// PAZ_CONDENSATION -   Paz condensation of mass and stiffness matrices
        /// Paz M. Dynamic condensation. AIAA J 1984;22(5):724-727.
        /// </summary>
        /// <param name="f"></param>
        public void PazCondensation(double f)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            //int N = M.RowCount;
            var n = DoFsToCondense.Length;
            Mc = new DenseMatrix(n);
            Kc = new DenseMatrix(n);

            var Drr = new DenseMatrix(DoF - n);
            var Drc = new DenseMatrix(DoF - n, n);

            var r = GenerateListOfDoFNotToCondense(DoF);

            var T = new DenseMatrix(DoF, n);

            var w2 = Sq(2 * Math.PI * f); // eigen frequency to eigenvalue

            for (var i = 0; i < DoF - n; i++)
            {
                for (var j = 0; j < DoF - n; j++) /* use only upper triangle of K,M */
                {
                    var ri = r[i];
                    var rj = r[j];
                    if (ri <= rj)
                    {
                        Drr[j, i] = Drr[i, j] = K[ri, rj] - w2 * M[ri, rj];
                    }
                    else
                    {
                        Drr[j, i] = Drr[i, j] = K[rj, ri] - w2 * M[rj, ri];
                    }
                }
            }

            for (var i = 0; i < DoF - n; i++)
            {
                for (var j = 0; j < n; j++) /* use only upper triangle of K,M */
                {
                    var ri = r[i];
                    var cj = DoFsToCondense[j];
                    if (ri < cj)
                    {
                        Drc[i, j] = K[ri, cj] - w2 * M[ri, cj];
                    }
                    else
                    {
                        Drc[i, j] = K[cj, ri] - w2 * M[cj, ri];
                    }
                }
            }

            var invDrrDrc = (DenseMatrix)Drr.Inverse() * Drc;
            //invAB(Drr, Drc,invDrrDrc); /* inv(Drr) * Drc */

            /* coordinate transformation matrix	*/
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    T[DoFsToCondense[i], j] = 0.0;
                }

                T[DoFsToCondense[i], i] = 1.0;
            }

            for (var i = 0; i < DoF - n; i++)
                for (var j = 0; j < n; j++)
                {
                    T[r[i], j] = -invDrrDrc[i, j];
                }

            xtAx(K, T, Kc); /* Kc = T' * K * T	*/
            xtAx(M, T, Mc); /* Mc = T' * M * T	*/
            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// MODAL_CONDENSATION -
        /// dynamic condensation of mass and stiffness matrices
        /// matches the response at a set of frequencies and modes
        /// WARNING: Kc and Mc may be ill-conditioned, and xyzsibly non-positive def.
        /// </summary>
        public void ModalCondensation()
        {
            double traceM = 0;

            var n = DoFsToCondense.Length;
            //int N = M.RowCount;
            var P = new DenseMatrix(n, n);

            for (var i = 0; i < n; i++) /* first n modal vectors at primary DoF's */
                for (var j = 0; j < n; j++)
                {
                    P[i, j] = Eigenvector[DoFsToCondense[i], MatchedCondenseModes[j]];
                }

            //PseudoInv(P, ref invP, 1e-9);

            var invP = (DenseMatrix)P.PseudoInverse();

            for (var i = 0; i < DoF; i++)
            {
                if (GlobalRestraints[i] == 0)
                {
                    traceM += M[i, i];
                }
            }

            Mc = (DenseMatrix)invP.Transpose() * invP;

            var traceMc = Mc.Trace();

            // create a diagonal matrix of eigenvalues:
            var ev = new DenseMatrix(MatchedCondenseModes.Length);
            for (var i = 0; i < MatchedCondenseModes.Length; i++)
            {
                ev[i, i] = omega2[i]; //  Sq(2.0 * Math.PI * eigenFreq[_modesToCondense[i]]);
            }

            Kc = (DenseMatrix)invP.Transpose() * ev * invP;
            Mc.Multiply(traceM / traceMc);
            Kc.Multiply(traceM / traceMc);
        }

        /// <summary>
        /// build a list of the not-condensed DoF's
        /// this is nifty: you only have to scan the range of numbers
        /// from 0 to DoF-1 ONCE, instead of the number of times the list-to condensed DoFs!
        /// Draw-back: the list of condensed DoF's must be sorted.
        /// So this routine sorts the list. If there already were sorted,
        /// the sort will go quickly, I presume.
        /// </summary>
        /// <param name="max">DoF's in the construction (nr. of rows in the stiffness matrix)</param>
        /// <returns>the list of non-condensed DoF's</returns>
        private static int[] GenerateListOfDoFNotToCondense(int max)
        {
            var DoFNotCondensed = new int[DoF];

            int k = 0;
            bool ok;

            for (int i = 0; i < DoF; i++)
            {
                ok = true;
                for (int j = 0; j < Cdof; j++)
                {
                    if (DoFsToCondense[j] == i)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    DoFNotCondensed[k++] = i;
                }
            }
            return DoFNotCondensed;
        }
    }
}
