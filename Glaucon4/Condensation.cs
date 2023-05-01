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

        private void ReadCondensationData()
        {
            //int node;

            if (CondensedNodes.Count > Nodes.Count)
            {
                throw new ArgumentOutOfRangeException(
                    "number of nodes with DoF's to condense larger than number of nodes");
            }

            DoFToCondense = new List<int>();
            foreach (var cn in CondensedNodes) // List was already set up
            {
                if (cn.NodeNr < 0 || cn.NodeNr > Nodes.Count)
                {
                    throw new IndexOutOfRangeException($"Node number ({cn.NodeNr}) out of range in condensation data.");
                }
                // set up the Vector of DoFs to condense:
                for (var j = 0; j < 6; j++)
                {
                    if (cn.DoFToCondense[j] != 0) // read 1's and 0's
                    {
                        // make a list of DoF's to condense:
                        DoFToCondense.Add(6 * (cn.NodeNr - 1) + j);
                    }
                }
            }

            // now, for each DoF to condense read the mode number.
            //  this is only for dynamic condensation!
            //if (Param.CondensationMethod == Dynamic && DoFToCondense.Count > Nodes.Count * 6)
            //{
            //    throw new ArgumentOutOfRangeException("DoFToCondense", DoFToCondense,
            //        $"The number of condensed degrees of freedom ({DoFToCondense.Count})" + Environment.NewLine +
            //        $" may not exceed the number of computed vibration modes ({nM})" + Environment.NewLine +
            //        "when using dynamic condensation.");
            //}

            // modes to animate:
            //NodesToCondense = new List<int>();
            //for (var i = 0; i < DoFToCondense.Count; i++)
            //{
            //    var m = buffer.ReadInt32() - 1;
            //    // validate mode numbers:
            //    if (m < 0 || m > nM - 1)
            //    {
            //        throw new IndexOutOfRangeException(
            //            $"Mode number ({NodesToCondense[i]}) out of range in condensation data.");
            //    }

            //    public List<int> MatchedCondenseModes;.Add(m);
            //}

            if (MatchedCondenseModes.Count != DoFToCondense.Count)
            {
                throw new ArgumentOutOfRangeException("ModeToCondense", MatchedCondenseModes,
                    $"Modes to condense {MatchedCondenseModes.Count} not equal to DoF's to condense {DoFToCondense.Count}");
            }
        }

        /// <summary>
        /// STATIC_CONDENSATION - of stiffness matrix from NxN to nxn
        /// Original name: static_condensation
        /// </summary>
        public void StaticCondensation()
        {
            var n = DoFToCondense.Count;
            Kc = new DenseMatrix(n);
            var Arr = new DenseMatrix(DoF - n);
            var Arc = new DenseMatrix(DoF - n, n);
            var r = GenerateListOfDoFToCondense(DoF);

            for (var i = 0; i < DoF - n; i++)
                for (var j = i; j < DoF - n; j++) /* use only upper triangle of K */
                {
                    var ri = r[i];
                    var rj = r[j];
                    if (ri <= rj)
                    {
                        Arr[j, i] = Arr[i, j] = K[ri, rj];
                    }
                }

            for (var i = 0; i < DoF - n; i++)
                for (var j = 0; j < n; j++) /* use only upper triangle of K */
                {
                    var ri = r[i];
                    var cj = DoFToCondense[j];
                    if (ri < cj)
                    {
                        Arc[i, j] = K[ri, cj];
                    }
                    else
                    {
                        Arc[i, j] = K[cj, ri];
                    }
                }

            xtinvAy(Arc, Arr, Arc, Kc); // uses symmetry
                                        // Kc = (DenseMatrix) Arc.Transpose().Inverse() * Arr * Arc;

            for (var i = 0; i < n; i++)
            {
                for (var j = i; j < n; j++) /* use only upper triangle of A */
                {
                    var ci = DoFToCondense[i];
                    var cj = DoFToCondense[j];
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
            var n = DoFToCondense.Count;
            Mc = new DenseMatrix(n);
            Kc = new DenseMatrix(n);

            var Drr = new DenseMatrix(DoF - n);
            var Drc = new DenseMatrix(DoF - n, n);

            var r = GenerateListOfDoFToCondense(DoF);

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
                    var cj = DoFToCondense[j];
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
                    T[DoFToCondense[i], j] = 0.0;
                }

                T[DoFToCondense[i], i] = 1.0;
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

            var n = DoFToCondense.Count;
            //int N = M.RowCount;
            var P = new DenseMatrix(n, n);

            for (var i = 0; i < n; i++) /* first n modal vectors at primary DoF's */
                for (var j = 0; j < n; j++)
                {
                    P[i, j] = Eigenvector[DoFToCondense[i], MatchedCondenseModes[j]];
                }

            //PseudoInv(P, ref invP, 1e-9);

            var invP = (DenseMatrix)P.PseudoInverse();

            for (var i = 0; i < DoF; i++)
            {
                if (Restraints[i] == 0)
                {
                    traceM += M[i, i];
                }
            }

            Mc = (DenseMatrix)invP.Transpose() * invP;

            var traceMc = Mc.Trace();

            // create a diagonal matrix of eigenvalues:
            var ev = new DenseMatrix(MatchedCondenseModes.Count);
            for (var i = 0; i < MatchedCondenseModes.Count; i++)
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
        private int[] GenerateListOfDoFToCondense(int max)
        {
            DoFToCondense.Sort();
            var n = DoFToCondense.Count;
            DoFToCondense.Add(max);
            var r = new int[max - n];
            var k = 0;
            var i1 = 0;
            foreach (var c in DoFToCondense)
            {
                for (; i1 < c; i1++)
                {
                    r[k++] = i1;
                }

                i1 = c + 1;
            }

            return r;
        }
    }
}
