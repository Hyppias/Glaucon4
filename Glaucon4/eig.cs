#region FileHeader
// Project: Glaucon4
// Filename:   eig.cs
// Last write: 4/30/2023 2:29:25 PM
// Creation:   4/24/2023 11:59:08 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.ComponentModel;
using System.Reflection;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        [XmlVector("Eigenfrequencies"), JsonProperty("Eigenfrequencies")]
        [Description("Eigenfrequencies, sorted from low to high")]
        public static DenseVector
            eigenFreq
        { get; set; }

        /// <summary>
        /// SUBSPACE - Find the lowest reqModes eigen-values, w, and eigen-vectors, V, of the
        /// general eigen-problem  ...  K V = w M V using sub-space / Jacobi iteration
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu  1 March 2007
        /// Bathe, Finite Element Procecures in Engineering Analysis, Prentice Hall, 1982. pag 678
        /// </summary>
        /// <param name="K">K is an DoF by DoF  symmetric real (stiffness) matrix</param>
        /// <param name="M">M is an DoF by DoF  symmetric positive definate real (mass) matrix</param>
        /// <param name="omega2"> w is a diagonal matrix of eigen-values</param>
        /// <param name="V">V is a  rectangular matrix of eigen-vectors</param>
        /// <param name="ok"></param>
        /// <param name="tol">convergence tolerance</param>
        public void Subspace(DenseVector omega2, DenseMatrix V, ref int ok, double tol)
        {
            //DenseMatrix Kb, Mb, Qb, Xb;
            //Debug.Assert(K.RowCount >= 12);

            // double[] d;
            double error, w_old = 0.0;

            var modes = Param.DynamicModesCount;
            // int[] idx;
            var m = omega2.Count;
            var n = K.RowCount;

            if (m > n)
            {
                throw new Exception("subspace: Number of eigen-values must be less than the problem dimension.\n" +
                    $"Desired number of eigen-values={m}\n Dimension of the problem= {n}");
            }

            //d = new double[n];
            var u = new double[n];
            var v = new double[n];
            var Kb = new DenseMatrix(m, m);
            var Mb = new DenseMatrix(m, m);
            var Xb = new DenseMatrix(n, m);
            var Qb = new DenseMatrix(m, m);
            var idx = new int[m];
            for (var i = 0; i < m; i++)
            {
                idx[i] = -1;
            }
            // reverse Bathe: modes = max( p / 2, p - 8): the original, requested nM
            ///modes = nM;

            // shift eigen-values by this much
            // Bathe, pag 570, (10.24)
            for (var i = 0; i < DoF; i++)
                for (var j = i; j < DoF; j++)
                {
                    K[i, j] += Shift * M[i, j];
                }

            WriteMatrix(EIGEN, "E", "K_beforeLDL", "K_beforeLDL", K);

            ldl_dcmp(K, n, u, v, v, true, false, ref ok); // use L D L' decomp: reduce, don't solve

            WriteMatrix(EIGEN, "E", "K_afterLDL", "K_afterLDL", K);

            var d = K.Diagonal().PointwiseDivide(M.Diagonal()).ToArray();
            // make a list of indices of the batheModes lowest d's, sorted from small to large.

            // bool have_it;
            var km_old = 0.0;
            for (var k = 0; k < m; k++)
            {
                var km = d[0];
                for (var i = 0; i < n; i++)
                {
                    if (km_old <= d[i] && d[i] <= km) // look for one between km_old and km
                    {
                        // yes: but maybe we have one already:
                        var have_it = false;
                        for (var j = 0; j < k; j++) // check whole idx if i already in it
                        {
                            if (i == idx[j])
                            {
                                have_it = true; // yes, already accounted for
                                break;
                            }
                        }

                        if (!have_it)
                        {
                            km = d[i];
                            idx[k] = i;
                        }
                    }
                }

                if (idx[k] == -1)
                {
                    var i = idx[0];
                    for (var j = 0; j < k; j++) // TODO: < k ?
                    {
                        if (i < idx[j])
                        {
                            i = idx[j];
                        }
                    }

                    idx[k] = i + 1;
                    km = d[i + 1];
                }

                km_old = km;
            }

            for (var k = 0; k < m; k++)
            {
                int i = 0, j = 0;
                V[idx[k], k] = 1.0;
                ok = idx[k] % 6;

                switch (ok)
                {
                    case 0:
                        i = 1;
                        j = 2;
                        break;
                    case 1:
                        i = -1;
                        j = 1;
                        break;
                    case 2:
                        i = -1;
                        j = -2;
                        break;
                    case 3:
                        i = 1;
                        j = 2;
                        break;
                    case 4:
                        i = -1;
                        j = 1;
                        break;
                    case 5:
                        i = -1;
                        j = -2;
                        break;
                }

                V[idx[k] + i, k] = 0.2;
                V[idx[k] + j, k] = 0.2;
            }

            WriteMatrix(EIGEN, "E", "V.dat", "V", V);

            iter = 0;
            do
            {
                // Begin sub-space iterations
                for (var k = 0; k < m; k++)
                {
                    // K Xb = M V	(12.10)
                    prodABj(M, V, v, k);
                    ldl_dcmp(K, n, u, v, d, false, true, ref ok); // LDL bk-sub

                    WriteMatrix(EIGEN, "E", "M.dat", "M", M);
                    WriteMatrix(EIGEN, "E", "K.dat", "K", K);

                    error = 1d;
                    ok = 1;
                    do
                    {
                        ldl_mprove(K, n, u, v, d, ref error, ref ok);
                    } while (ok != 0);

                    for (var i = 0; i < n; i++)
                    {
                        Xb[i, k] = d[i];
                    }
                }

                // DO NOT replace with MathNet routines!!
                xtAx(K, Xb, Kb); // Kb = Xb' K Xb (12.11)
                xtAx(M, Xb, Mb); // Mb = Xb' M Xb (12.12)

                Jacobi(Kb, Mb, omega2, Qb); // (12.13)

                //V =  Xb * Qb;
                // V = (DenseMatrix) Xb.Multiply(Qb);

                prodAB(Xb, Qb, V, DoF, m, m); // batheModes, batheModes);     // V = Xb Qb (12.14)
                Eigsort(omega2, V, m);
                //EigSort(omega2, V, 0, omega2.Count - 1);

                if (omega2[modes - 1].AlmostEqual(0.0, 5))
                {
                    throw new Exception($"subspace: Zero frequency found: w[{modes}] = {omega2[modes - 1]}");
                }

                error = Math.Abs(omega2[modes - 1] - w_old) / omega2[modes - 1];

                iter++;
                w_old = omega2[modes - 1];

                if (iter > 1000)
                {
                    throw new Exception("To many iterations");
                }
            } while (error > tol); // End   sub-space iterations

            for (var k = 0; k < m; k++)
            {
                // shift eigen-values
                if (omega2[k] > Shift)
                {
                    omega2[k] = omega2[k] - Shift;
                }
                else
                {
                    omega2[k] = Shift - omega2[k];
                }
            }

            ok = Sturm(m, omega2[modes - 1] + tol);

            // de-shift the eigenvalues
            // Bathe pag. 570
            for (var i = 0; i < n; i++)
                for (var j = i; j < n; j++)
                {
                    K[i, j] -= Shift * M[i, j];
                }
        }

        /// <summary>
        /// JACOBI - Find all eigen-values, E, and eigen-vectors, V,
        /// of the general eigen-problem  K V = E M V
        /// using Jacobi iteration, with efficient matrix rotations.
        /// K is a symmetric real (stiffness) matrix
        /// M is a symmetric positive definate real (mass) matrix
        /// E is a diagonal matrix of eigen-values
        /// V is a  square  matrix of eigen-vectors
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu  1 March 2007
        /// Bathe, Klaus-Juergen, Finite Element Procecures,
        /// PHI Learning Private Ltd. 2015 (c) 1995, page 912
        /// </summary>
        /// <param name="K">K is a symmetric real (stiffness) matrixx</param>
        /// <param name="M">M is a symmetric positive definate real (mass) matrix</param>
        /// <param name="Eigenvalues">Eigenvalues is a diagonal matrix of eigen-values</param>
        /// <param name="Eigenvectors">Eigenvectors is a  square  matrix of eigen-vectors</param>
        private void Jacobi(DenseMatrix K, DenseMatrix M, DenseVector Eigenvalues,
            DenseMatrix Eigenvectors)
        {
            var n = K.RowCount;
            //Kii = Kjj = Kij = Mii = Mjj = Mij = Vki = Vkj = 0.0;
            //Debug.WriteLine("");
            // zero the upper and lower triangles:
            for (var i = 0; i < n; i++)
                for (var j = i + 1; j < n; j++)
                {
                    Eigenvectors[i, j] = Eigenvectors[j, i] = 0.0;
                }

            for (var d = 0; d < n; d++)
            {
                Eigenvectors[d, d] = 1.0;
            }

            for (iter = 1; iter <= 2 * n; iter++)
            {
                // Begin Sweep Iteration

                var tol = Math.Pow(0.01, 2 * iter);
                // tol = 0.0;

                for (var d = 0; d < n - 1; d++)
                {
                    // sweep along upper diagonals
                    for (var i = 0; i < n - d - 1; i++)
                    {
                        // row
                        var j = i + d + 1; // column

                        var Kij = K[i, j];
                        var Mij = M[i, j];

                        if (Kij * Kij / (K[i, i] * K[j, j]) > tol ||
                            Mij * Mij / (M[i, i] * M[j, j]) > tol)
                        {
                            // do a rotation
                            double gamma;
                            var Kii = K[i, i] * Mij - Kij * M[i, i];
                            var Kjj = K[j, j] * Mij - Kij * M[j, j];
                            var s = K[i, i] * M[j, j] - K[j, j] * M[i, i];

                            if (s >= 0.0)
                            {
                                gamma = 0.5 * s + Math.Sqrt(0.25 * s * s + Kii * Kjj);
                            }
                            else
                            {
                                gamma = 0.5 * s - Math.Sqrt(0.25 * s * s + Kii * Kjj);
                            }

                            //Debug.WriteLine($"iter = {iter:d2} d = {d:d2} i = {i:d2} j = {j:d2} Kij = {Kij:e6} Mij = {Mij:e6} Kii = {Kii:e6} Kjj = {Kjj:e6} s = {s:e6} gamma = {gamma:e6}");
                            var alpha = Kjj / gamma;
                            var beta = -Kii / gamma;

                            Rotate(K, n, alpha, beta, i, j); // make Kij zero
                            Rotate(M, n, alpha, beta, i, j); // make Mij zero

                            for (var k = 0; k < n; k++)
                            {
                                //  update eigen-vectors  Eigenvectors = Eigenvectors * P
                                var vki = Eigenvectors[k, i];
                                var vkj = Eigenvectors[k, j];
                                Eigenvectors[k, i] = vki + beta * vkj;
                                Eigenvectors[k, j] = vkj + alpha * vki;
                            }
                        } // rotations complete
                    } // row
                } // diagonal
            } // End Sweep Iteration

            for (var j = 0; j < n; j++)
            {
                // scale eigen-vectors
                var Mjj = Math.Sqrt(M[j, j]);
                for (var i = 0; i < n; i++)
                {
                    Eigenvectors[i, j] /= Mjj;
                }
            }

            for (var j = 0; j < n; j++)
            {
                Eigenvalues[j] = K[j, j] / M[j, j]; // eigen-values
            }
        }

        /// <summary>
        /// ROTATE - rotate an n by n symmetric matrix A such that A[i,j] = A[j,i] = 0
        /// A = P' * A * P  where diag(P) = 1 and P[i,j] = alpha and P[j,i] = beta.
        /// Since P is sparse, this matrix multiplcation can be done efficiently.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="n"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void Rotate(DenseMatrix A, int n, double alpha, double beta, int i, int j)
        {
            ///double Aii, Ajj, Aij;  // elements of A	

            // double[] Ai, Aj;       // i-th and j-th rows of A
            //int k;

            var Ai = new double[n];
            var Aj = new double[n];

            for (var k = 0; k < n; k++)
            {
                Ai[k] = A[i, k];
                Aj[k] = A[j, k];
            }

            var Aii = A[i, i];
            var Ajj = A[j, j];
            var Aij = A[i, j];

            A[i, i] = Aii + 2 * beta * Aij + beta * beta * Ajj;
            A[j, j] = Ajj + 2 * alpha * Aij + alpha * alpha * Aii;

            for (var k = 0; k < n; k++)
            {
                if (k != i && k != j)
                {
                    A[k, i] = A[i, k] = Ai[k] + beta * Aj[k];
                    A[k, j] = A[j, k] = Aj[k] + alpha * Ai[k];
                }
            }

            A[j, i] = A[i, j] = 0;
        }

        /// <summary>
        /// STODOLA  -  calculate the lowest reqModes eigen-values and eigen-vectors of the
        /// generalized eigen-problem, K v = w M v, using a matrix iteration approach
        /// with shifting
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu  12 Jul 2001
        /// </summary>
        /// <param name="K">stiffness matrix</param>
        /// <param name="M">mas matrix</param>
        /// <param name="m">Bathe modes</param>
        /// <param name="reqModes">required modes</param>
        /// <param name="w"></param>
        /// <param name="V"></param>
        /// <param name="tol"></param>
        /// <param name="shift"></param>
        /// <param name="iter"></param>
        /// <param name="ok"></param>
        public void Stodola(DenseVector w, DenseMatrix V, ref int ok, double tol)
        {
            // DenseVector v;       // trial eigen-vector vectors

            //double
            //    d_min = 0.0, // minimum value of D[i,i]		
            //    d_max = 0.0,    // maximum value of D[i,i]		
            //    d_old,    // previous extreme value of D[i,i]	

            //    vMv,            // factor for mass normalization	
            //    RQ, RQold = 0.0,// Raleigh quotient			
            //    error;

            var i_ex = int.MaxValue; // location of minimum value of D[i,i]	

            var m = w.Count;
            var n = K.RowCount;
            var D = new DenseMatrix(n); // the dynamics matrix, D = K^(-1) M	
            var d = new DenseVector(n); // columns of the D, M, and V matrices	
            var u = new DenseVector(n); // trial eigen-vector vectors	
                                            //v = new DenseVector(n);
            var c = new double[m];

            // reverse Bathe modes: really the original nM
            var modes = Param.DynamicModesCount; // number of desireable modes	
            try
            {
                DenseVector v;
                // shift eigen-values by this much
                for (var i = 0; i < n; i++)
                    for (var j = i; j < n; j++)
                    {
                        K[i, j] += Shift * M[i, j];
                    }

                ldl_dcmp(K, n, u, null, null, true, false, ref ok); // use L D L' decomp	
                if (ok < 0)
                {
                    throw new Exception("Make sure that all six rigid body translation are restrained.");
                }
                // calculate  D = K^(-1) M

                for (var j = 0; j < n; j++)
                {
                    // for (int i = 0; i < n; i++)
                    //     v[i] = M[i, j];
                    //M.Column(j).CopyTo(v);
                    v = (DenseVector)M.Column(j);
                    ldl_dcmp(K, n, u, v, d, false, true, ref ok); // L D L' bk-sub

                    var error = 1.0;
                    ok = 1;
                    do
                    {
                        ldl_mprove(K, n, u, v, d, ref error, ref ok);
                    } while (ok != 0);

                    //d.CopyTo(D.Column(j));
                    for (var i = 0; i < n; i++)
                    {
                        D[i, j] = d[i];
                    }
                }
#if EIGEN
                WriteMatrix("E", "D.dat", "D", D);
#endif
                iter = 0;
                var d_max = D.Diagonal().Maximum();
                //for (int i = 0; i < n; i++)
                //    if (D[i, i] > d_max)
                //        d_max = D[i, i];

                var d_old = d_max;
                //for (int i = 0; i < n; i++)
                //    if (D[i, i] < d_min)
                //        d_min = D[i, i];
                var d_min = D.Diagonal().Minimum();
                for (var k = 0; k < m; k++)
                {
                    // loop over lowest batheModes modes
                    double RQold;
                    d_max = d_min;
                    for (var i = 0; i < n; i++)
                    {
                        // initial guess
                        u[i] = 0.0;
                        if (D[i, i] < d_old && D[i, i] > d_max)
                        {
                            d_max = D[i, i];
                            i_ex = i;
                        }
                    }

                    u[i_ex] = 1.0;
                    u[i_ex + 1] = 1e-4;
                    d_old = d_max;

                    var vMv = xtAy(u, M, u, n, d); // mass-normalize

                    u /= Math.Sqrt(vMv);
                    //for (int i = 0; i < n; i++)
                    //    u[i] /= Math.Sqrt(vMv);

                    for (var j = 0; j < k; j++) // E
                    {
                        // purge lower modes
                        v = (DenseVector)V.Column(j);
                        //for (int i = 0; i < n; i++)
                        //    v[i] = V[i, j];
                        c[j] = xtAy(v, M, u, n, d);
                    }

                    for (var j = 0; j < k; j++)
                        for (var i = 0; i < n; i++)
                        {
                            u[i] -= c[j] * V[i, j];
                        }

                    vMv = xtAy(u, M, u, n, d); // mass-normalize
                                               //u /= Math.Sqrt(vMv);
                    for (var i = 0; i < n; i++)
                    {
                        u[i] /= Math.Sqrt(vMv);
                    }

                    var RQ = xtAy(u, K, u, n, d); // Raleigh quotient

                    do
                    {
                        // iterate	
                        //for (int i = 0; i < n; i++)
                        //{           // v = D u	
                        //    v[i] = 0.0;
                        //    for (int j = 0; j < n; j++)
                        //        v[i] += D[i, j] * u[j];
                        //}
                        v = D * u;
                        vMv = xtAy(v, M, v, n, d); // mass-normalize
                                                   //v /= Math.Sqrt(vMv);
                                                   //for (int i = 0; i < n; i++)
                                                   //    v[i] /= Math.Sqrt(vMv);
                        v /= Math.Sqrt(vMv);

                        for (var j = 0; j < k; j++)
                        {
                            // purge lower modes
                            //V.Column(j).CopyTo(u);
                            for (var i = 0; i < n; i++)
                            {
                                u[i] = V[i, j];
                            }

                            c[j] = xtAy(u, M, v, n, d);
                        }

                        for (var j = 0; j < k; j++)
                            for (var i = 0; i < n; i++)
                            {
                                v[i] -= c[j] * V[i, j];
                            }

                        vMv = xtAy(v, M, v, n, d); // mass-normalize
                                                   //(v / Math.Sqrt(vMv)).CopyTo(u);
                                                   //for (int i = 0; i < n; i++)
                                                   //    u[i] = v[i] / Math.Sqrt(vMv);
                        u = v / Math.Sqrt(vMv);

                        RQold = RQ;
                        RQ = xtAy(u, K, u, n, d); // Raleigh quotient
                        iter++;

                        if (iter > 1000)
                        {
                            throw new Exception(
                                $"{MethodBase.GetCurrentMethod().Name}: Iteration limit exceeded:  rel. error = {Math.Abs(RQ - RQold) / RQ} > {tol}");
                        }
                    } while (Math.Abs(RQ - RQold) / RQ > tol);

                    //v.CopyTo(V.Column(k));
                    for (var i = 0; i < n; i++)
                    {
                        V[i, k] = v[i];
                    }

                    w[k] = xtAy(u, K, u, n, d);
                    if (w[k] > Shift)
                    {
                        w[k] -= Shift;
                    }
                    else
                    {
                        w[k] = Shift - w[k];
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new Exception($"{ex.Message} in {MethodBase.GetCurrentMethod().Name}", ex.InnerException);
            }

            Eigsort(w, V, m);
            //EigSort(w, V, 0, w.Count - 1);
            ok = Sturm(m, w[modes - 1] + tol);
        }

        /// <summary>
        /// EIGSORT  -  Given the eigenvallues e[1..reqModes] and eigenvectors v[1..DoF,1..reqModes],
        /// this routine sorts the eigenvalues into ascending order, and rearranges
        /// the columns of v correspondingly.  The CondensationMethod is straight insertion.
        /// Adapted from Numerical Recipes in C, Ch 11
        /// </summary>
        /// <param name="eigenvalues">eigenvalues</param>
        /// <param name="eigenvectors">eigenvectors</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void EigSort(DenseVector eigenvalues, DenseMatrix eigenvectors, int left, int right)
        {
            if (left < right)
            {
                var pivot = Partition(eigenvalues, eigenvectors, left, right);

                if (pivot > 1)
                {
                    EigSort(eigenvalues, eigenvectors, left, pivot - 1);
                }

                if (pivot + 1 < right)
                {
                    EigSort(eigenvalues, eigenvectors, pivot + 1, right);
                }
            }
        }

        private static int Partition(DenseVector eigenvalues, DenseMatrix eigenvectors, int left, int right)
        {
            var pivot = eigenvalues[left];
            while (true)
            {
                while (eigenvalues[left] < pivot)
                {
                    left++;
                }

                while (eigenvalues[right] > pivot)
                {
                    right--;
                }

                if (left < right)
                {
                    if (eigenvalues[left] == eigenvalues[right])
                    {
                        return right;
                    }

                    var temp = eigenvalues[left];
                    eigenvalues[left] = eigenvalues[right];
                    eigenvalues[right] = temp;

                    for (var j = 0; j < eigenvectors.ColumnCount; j++)
                    {
                        temp = eigenvectors[j, left];
                        eigenvectors[j, left] = eigenvectors[j, right];
                        eigenvectors[j, right] = temp;
                    }
                }
                else
                {
                    return right;
                }
            }
        }

        private void Eigsort(DenseVector eigenvalues, DenseMatrix eigenvectors, int m)
        {
            for (var i = 0; i < m - 1; i++)
            {
                var k = i;
                var p = eigenvalues[k];
                for (var j = i + 1; j < m; j++)
                {
                    if (eigenvalues[j] <= p)
                    {
                        p = eigenvalues[k = j]; // find smallest eigen-value
                    }
                }

                if (k != i)
                {
                    eigenvalues[k] = eigenvalues[i]; // swap eigen-values	
                    eigenvalues[i] = p;
                    for (var j = 0; j < DoF; j++)
                    {
                        // swap eigen-vectors simultaneously	
                        p = eigenvectors[j, i];
                        eigenvectors[j, i] = eigenvectors[j, k];
                        eigenvectors[j, k] = p;
                    }
                }
            }
        }

        /// <summary>
        /// STURM  -  Determine the number of eigenvalues, w, of the general eigen-problem
        /// K V = w M V which are below the value ws,
        /// K is an DoF by DoF  symmetric real (stiffness) matrix
        /// M is an DoF by DoF  symmetric positive definate real (mass) matrix
        /// reqModes is the number of required modes
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu  30 Aug 2001
        /// Bathe, Finite Element Procecures in Engineering Analysis, Prentice Hall, 1982
        /// </summary>
        /// <param name="K">System stiffness matrix</param>
        /// <param name="M">System mass matrix</param>
        /// <param name="batheModes">required modes according to Bathe</param>
        /// <param name="limitEigenvalue"></param>
        private int Sturm(int batheModes, double limitEigenvalue)
        {
            var ok = 0;

            var m = K.RowCount;
            var d = new DenseVector(m);

            var modes = Param.DynamicModesCount;

            var shiftedLimitEigenvalue = limitEigenvalue + Shift; // shift [K]	upper
            for (var i = 0; i < m; i++)
                for (var j = i; j < m; j++)
                {
                    K[i, j] -= shiftedLimitEigenvalue * M[i, j];
                }

            ldl_dcmp(K, m, d, d, d, true, false, ref ok);

            if (-ok > modes)
            {
                throw new ArgumentException(
                    $"{MethodBase.GetCurrentMethod().Name}: {-ok - modes} modes were not found.\n" +
                    "Try increasing the number of modes in\n" +
                    $"order to get the missing modes below {Math.Sqrt(limitEigenvalue) / (2.0 * Math.PI)} Hz.");
            }

            for (var i = 0; i < m; i++)
                for (var j = i; j < m; j++) // upper K and M  only
                {
                    K[i, j] += shiftedLimitEigenvalue * M[i, j];
                }

            return ok;
        }

        /// <summary>
        /// CHECK_NON_NEGATIVE -  checks that a value is non-negative
        /// </summary>
        /// <param name="x"></param>
        /// <param name="i">index</param>
        private void Check_non_negative(double x, int i)
        {
            if (x <= 1.0e-100)
            {
                throw new Exception($" value {x} is less than or equal to zero  i = {i} ");
            }
        }
    }
}
