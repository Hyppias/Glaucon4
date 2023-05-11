#region FileHeader
// Project: Glaucon4
// Filename:   Matrix.cs
// Last write: 4/30/2023 2:29:31 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

#define CONDENSE
using System;
using System.Reflection;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        /// <summary>
        /// GAUSSJ
        /// Linear equation solution by Gauss-Jordan elimination, [A,X]=[B] above. A[1..n,1..n]
        /// is the input matrix. B[1..n,1..m] is input containing the m right-hand side vectors. On
        /// output, a is replaced by its matrix inverse, and B is replaced by the corresponding set of solution
        /// vectors.
        /// </summary>
        /// <param name="A">Stiffness matrix</param>
        /// <param name="n">DoF</param>
        /// <param name="B">Force vector</param>
        /// <param name="B">Solution vector</param>
        public void Gaussj(double[,] A, int n, double[,] B, int m)
        {
            int icol = 0, irow = 0;

            // The integer arrays ipiv, indxr, and indxc are used for bookkeeping on the pivoting.

            var indxc = new int[n];
            var indxr = new int[n];
            var ipiv = new int[n];

            //  This is the main loop over the columns to be reduced.

            for (var i = 0; i < n; i++)
            {
                var big = 0.0d;
                //  This is the outer loop for the search for a pivot element.

                for (var j = 0; j < n; j++)
                {
                    if (ipiv[j] != 1)
                    {
                        for (var k = 0; k < n; k++)
                        {
                            if (ipiv[k] == 0)
                            {
                                if (Math.Abs(A[j, k]) >= big)
                                {
                                    big = Math.Abs(A[j, k]);
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (ipiv[k] > 1)
                            {
                                throw new Exception("50 gaussj: Singular Matrix-1");
                            }
                        }
                    }
                }

                ++ipiv[icol];

                // We now have the pivot element, so we interchange rows, if needed, to put the pivot
                // element on the diagonal. The columns are not physically interchanged, only relabeled:
                // indxc[i], the column of the ith pivot element, is the ith column that is reduced, while
                // indxr[i] is the row in which that pivot element was originally located. If indxr[i] =
                // indxc[i] there is an implied column interchange. With this form of bookkeeping, the
                // solution b's will end up in the correct order, and the inverse matrix will be scrambled
                // by columns.

                if (irow != icol)
                {
                    for (var l = 0; l < n; l++)
                    {
                        Swap(ref A[irow, l], ref A[icol, l]);
                    }

                    for (var l = 0; l < m; l++)
                    {
                        Swap(ref B[irow, l], ref B[icol, l]);
                    }
                }

                indxr[i] = irow;
                indxc[i] = icol;

                // We are now ready to divide the pivot row by the by the pivot element, located at irow,icol

                if (A[icol, icol].AlmostEqual(0.0))
                {
                    throw new Exception("51 gaussj: Singular Matrix-2");
                }

                var pivinv = 1.0 / A[icol, icol];
                A[icol, icol] = 1.0;
                for (var l = 0; l < n; l++)
                {
                    A[icol, l] *= pivinv;
                }

                for (var l = 0; l < m; l++)
                {
                    B[icol, l] *= pivinv;
                }

                //  Next, we reduce the rows ... except for the pivot one, of course.

                for (var ll = 0; ll < n; ll++)
                {
                    if (ll != icol)
                    {
                        var dum = A[ll, icol];
                        A[ll, icol] = 0.0;
                        for (var l = 0; l < n; l++)
                        {
                            A[ll, l] -= A[icol, l] * dum;
                        }

                        for (var l = 0; l < m; l++)
                        {
                            B[ll, l] -= B[icol, l] * dum;
                        }
                    }
                }
            }

            // This is the end of the main loop over columns of the reduction. It only remains to unscram-
            // ble the solution in view of the column interchanges. We do this by interchanging pairs of
            // columns in the reverse order that the permutation was built up.

            for (var l = n - 1; l >= 0; l--)
            {
                if (indxr[l] != indxc[l])
                {
                    for (var k = 0; k < n; k++)
                    {
                        Swap(ref A[k, indxr[l]], ref A[k, indxc[l]]);
                    }
                }
            }
        }

        private void Swap(ref double a, ref double b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// LU_DCMP
        /// Solves[A]{x} = {b}
        /// simply and efficiently by performing an
        /// LU - decomposition of[A].  No pivoting is performed.
        /// usage:  double** A, * b;
        /// int n, reduce, solve, pd;
        /// lu_dcmp(A, n, b, reduce, solve );
        /// </summary>
        /// <param name="A">
        /// the system matrix, and it's LU- reduction,
        /// a diagonally dominant matrix of dimension[1..n, 1..n].
        /// [A] is replaced by the LU - reduction of itself.
        /// </param>
        /// <param name="b">
        /// the right hand side vector, and the solution vector.
        /// {b} is a r.h.s.vector of dimension [1..n].
        /// {b} is updated using [LU] and then back-substitution is done to obtain {x}.
        /// {b} is replaced by { x }
        /// </param>
        /// <param name="reduce">1: do a forward reduction; 0: don't do the reduction </param>
        /// <param name="solve">1: do a back substitution for {x};  0: do no bk-sub'n</param>
        public static void lu_dcmp(DenseMatrix A, DenseVector b, bool reduce, bool solve)
        {
            //double pivot;       // a diagonal element of [A]		
            var n = A.RowCount;

            if (reduce)
            {
                // forward reduction of [A]	

                for (var k = 0; k < n; k++)
                {
                    double pivot;

                    if ((pivot = A[k, k]).AlmostEqual(0.0))
                    {
                        throw new Exception(
                            $"{MethodBase.GetCurrentMethod().Name}: zero found on the diagonal of the stiffness matrix");
                    }

                    for (var i = k + 1; i < n; i++)
                    {
                        A[i, k] /= pivot;
                        for (var j = k + 1; j < n; j++)
                        {
                            A[i, j] -= A[i, k] * A[k, j];
                        }
                    }
                }
            } // the forward reduction of [A] is now complete	

            if (solve)
            {
                // back substitution to solve for {x}	

                // {b} is run through the same forward reduction as was [A]	

                for (var k = 0; k < n; k++)
                    for (var i = k + 1; i < n; i++)
                    {
                        b[i] -= A[i, k] * b[k];
                    }

                // now back substitution is conducted on {b};  [A] is preserved

                for (var j = n - 1; j >= 1; j--)
                    for (var i = 0; i <= j - 1; i++)
                    {
                        b[i] -= b[j] * A[i, j] / A[j, j];
                    }

                // finally we solve for the {x} vector			

                for (var i = 0; i < n; i++)
                {
                    b[i] /= A[i, i];
                }
            }
        }

        /// <summary>
        /// LDL_DCMP  -  Solves [A]{x} = {b} simply and efficiently by performing an
        /// L D L' - decomposition of [A].  No pivoting is performed.
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu  9 Oct 2001
        /// Bathe, Finite Element Procecures in Engineering Analysis, Prentice Hall, 1982
        /// <param name="A">
        ///    the system matrix, and L of the L D L' decomp.
        ///    [A] is a symmetric diagonally-dominant matrix of dimension [1..n,1..n].
        /// </param>
        /// <param name="n">the dimension of the matrix</param>
        /// <param name="d">the right hand side vector, and the solution vector </param>
        /// <param name="b">
        ///    the right hand side vector.
        ///    {b} is a r.h.s. vector of dimension [1..n].
        ///    {b} is updated using L D L' and then back-substitution is done to obtain {x}
        ///    {b} is returned unchanged.  ldl_dcmp(A,n,d,x,x,1,1) is valid.
        ///    The lower triangle of [A] is replaced by the lower triangle L of the
        ///    L D L' reduction.  The diagonal of D is returned in the vector {d}
        /// </param>
        /// <param name="x">the solution vector	</param>
        /// <param name="reduce">1: do a forward reduction; 0: don't do the reduction </param>
        /// <param name="solve">1: do a back substitution for {x};  0: do no bk-sub'n</param>
        /// <param name="pd">1: positive diagonal  and  successful LU decomp'n	</param>
        /// </summary>
        public static void ldl_dcmp(DenseMatrix A, int n, DenseVector d, DenseVector b, DenseVector x,
            bool reduce, bool solve, ref int pd)
        {
            pd = 0; // number of negative elements on the diagonal of D

            if (reduce)
            {
                // forward column-wise reduction of [A]	
                //int m ;
                for (var j = 0; j < n; j++)
                {
                    var m = 0;
                    for (var i = 0; i < j; i++) // scan the sky-line	
                    {
                        if (A[i, j].AlmostEqual(0.0, 10)) //< 1e-13 && A[i,j] > -1e-13)
                        {
                            ++m;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (var i = m; i < j; i++)
                    {
                        A[j, i] = A[i, j];
                        for (var k = m; k < i; k++)
                        {
                            A[j, i] -= A[j, k] * A[i, k];
                        }
                    }

                    d[j] = A[j, j];
                    for (var i = m; i < j; i++)
                    {
                        d[j] -= A[j, i] * A[j, i] / d[i];
                    }

                    for (var i = m; i < j; i++)
                    {
                        A[j, i] /= d[i];
                    }

                    if (d[j].AlmostEqual(0.0))
                    {
                        throw new Exception($" ldl_dcmp(): zero found on diagonal d[{j}] = {d[j]}");
                    }

                    if (d[j] < 0.0)
                    {
                        pd--;
                    }
                }
            } // the forward reduction of [A] is now complete	

            if (solve)
            {
                // back substitution to solve for {x}
                // {x} is run through the same forward reduction as was [A]

                for (var i = 0; i < n; i++)
                {
                    x[i] = b[i];
                    for (var j = 0; j < i; j++)
                    {
                        x[i] -= A[i, j] * x[j];
                    }
                }

                for (var i = 0; i < n; i++)
                {
                    x[i] /= d[i];
                }

                // now back substitution is conducted on {x};  [A] is preserved

                for (var i = n - 1; i > 0; i--)
                    for (var j = 0; j < i; j++)
                    {
                        x[j] -= A[i, j] * x[i];
                    }
            }
        }

        /// <summary>
        /// LDL_MPROVE  Improves a solution vector x[1..n] of the linear set of equations
        /// [A]{x} = {b}.  The matrix A[1..n,1..n], and the vectors b[1..n] and x[1..n]
        /// are input, as is the dimension n.   The matrix [A] is the L D L'
        /// decomposition of the original system matrix, as returned by ldl_dcmp().
        /// Also input is the diagonal vector, {d} of [D] of the L D L' decompositon.
        /// On output, only {x} is modified to an improved set of values.
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu  4 May 2001
        /// </summary>
        /// ///
        /// <param name="A">the system matrix, and L of the L D L' decomp.</param>
        /// <param name="n">the dimension of the matrix</param>
        /// <param name="d">the right hand side vector, and the solution vector </param>
        /// <param name="b">the right hand side vector</param>
        /// <param name="x">the solution vector	</param>
        /// <param name="rms_resid"></param>
        /// <param name="ok"></param>
        private static void ldl_mprove(DenseMatrix A, int n, DenseVector d, DenseVector b, DenseVector x,
            ref double rms_resid, ref int ok)
        {
            //double sdp;        // accumulate the r.h.s. in double precision                	  	
            var rms_resid_new = 0d; // the RMS error of the mprvd solution	

            var pd = 0;

            var resid = new DenseVector(n); // the residual error	

            for (var i = 0; i < n; i++)
            {
                // calculate the r.h.s. of  [A]{r} = {b} - [A]{x+r}
                var sdp = b[i];
                for (var j = 0; j < n; j++)
                {
                    // A in upper triangle only
                    if (i <= j)
                    {
                        sdp -= A[i, j] * x[j];
                    }
                    else
                    {
                        sdp -= A[j, i] * x[j];
                    }
                }

                resid[i] = sdp;
            }

            // solve for the error term
            ldl_dcmp(A, n, d, resid, resid, false, true, ref pd);
            rms_resid_new = 0d;
            for (var i = 0; i < n; i++)
            {
                rms_resid_new += resid[i] * resid[i];
            }

            rms_resid_new = Math.Sqrt(rms_resid_new / n);

            ok = 0;
            if (rms_resid_new / rms_resid < 0.90d)
            {
                // good improvement
                // subtract the error from the old solution
                for (var i = 0; i < n; i++)
                {
                    x[i] += resid[i];
                }

                rms_resid = rms_resid_new;
                ok = 1; // the solution has improved		
            }
        }

        /// <summary>
        /// LDL_DCMP_PM  -  Solves partitioned matrix equations
        /// [A_qq]{x_q} + [A_qr]{x_r} = {b_q}
        /// [A_rq]{x_q} + [A_rr]{x_r} = {b_r}+{c_r}
        /// where {b_q}, {b_r}, and {x_r} are known and
        /// where {x_q} and {c_r} are unknown
        /// via L D L' - decomposition of [A_qq].  No pivoting is performed.
        /// [A] is a symmetric diagonally-dominant matrix of dimension [1..n,1..n].
        /// {b} is a r.h.s. vector of dimension [1..n].
        /// {b} is updated using L D L' and then back-substitution is done to obtain {x}
        /// {b_q} and {b_r}  are returned unchanged.
        /// {c_r} is returned as a vector of [1..n] with {c_q}=0.
        /// {restraintsNot} is a vector of the indexes of known values {b_q}
        /// {r} is a vector of the indexes of known values {x_r}
        /// The lower triangle of [A_qq] is replaced by the lower triangle L of its
        /// L D L' reduction.  The diagonal of D is returned in the vector {d}
        /// usage: double **A, *d, *b, *x;
        /// int   n, reduce, solve, pd;
        /// ldl_dcmp_pm ( A, n, d, b, x, c, restraintsNot, r, reduce, solve, &pd );
        /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu
        /// Bathe, Finite Element Procecures in Engineering Analysis, Prentice Hall, 1982
        /// 2014-05-14
        /// </summary>
        /// /// ///
        /// <param name="A">the system matrix, and L of the L D L' decomp.</param>
        /// <param name="n">the dimension of the matrix</param>
        /// <param name="diag">diagonal of D in the  L D L' - decomp'n </param>
        /// <param name="b">the right hand side vector</param>
        /// <param name="x">part of the solution vector</param>
        /// <param name="c"> the part of the solution vector in the rhs </param>
        /// <param name="reduce">reduce=1 ; reduce=0 otherwise	</param>
        /// <param name="solve"> 1: do a back substitution for {x}; 0: don't </param>
        /// <param name="pd">1: PositiveDefiniteness: definite matrix and successful L D L' decomp'n</param>
        private void ldl_dcmp_pm(double[,] A, int n, double[] diag, double[] b, double[] x,
            double[] c, bool reduce, bool solve, ref int pd) // pd was ok!
        {
            pd = 0; // number of negative elements on the diagonal of D

            if (reduce)
            {
                // forward column-wise reduction of [A]	
                for (var j = 0; j < n; j++)
                {
                    diag[j] = 0.0;

                    if (GlobalRestraints[j] ==0)
                    {
                        // reduce column j, except where restraintsNot[i]==0	
                        var m = 0;
                        for (var i = 0; i < j; i++) // scan the sky-line	
                        {
                            if (A[i, j].AlmostEqual(0.0))
                            {
                                ++m;
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (var i = m; i < j; i++)
                        {
                            if (GlobalRestraints[i] == 0)
                            {
                                A[j, i] = A[i, j];
                                for (var k = m; k < i; k++)
                                {
                                    if (GlobalRestraints[k] == 0)
                                    {
                                        A[j, i] -= A[j, k] * A[i, k];
                                    }
                                }
                            }
                        }

                        diag[j] = A[j, j];
                        for (var i = m; i < j; i++)
                        {
                            if (GlobalRestraints[i] == 0)
                            {
                                diag[j] -= A[j, i] * A[j, i] / diag[i];
                            }
                        }

                        for (var i = m; i < j; i++)
                        {
                            if (GlobalRestraints[i] == 0)
                            {
                                A[j, i] /= diag[i];
                            }
                        }

                        if (diag[j].AlmostEqual(0.0))
                        {
                            throw new Exception(
                                $"{MethodBase.GetCurrentMethod().Name}: zero found on diagonal diag[{j}] = {diag[j]}");
                        }

                        if (diag[j] < 0.0)
                        {
                            pd--;
                        }
                    }
                }
            } // the forward reduction of [A] is now complete	

            if (solve)
            {
                // back substitution to solve for {x}

                for (var i = 0; i < n; i++)
                {
                    if (GlobalRestraints[i] == 0)
                    {
                        x[i] = b[i];
                        for (var j = 0; j < n; j++)
                        {
                            if (GlobalRestraints[j] == 1)
                            {
                                x[i] -= A[i, j] * x[j];
                            }
                        }
                    }
                }

                // {x} is run through the same forward reduction as was [A]
                for (var i = 0; i < n; i++)
                {
                    if (GlobalRestraints[i] == 0)
                    {
                        for (var j = 0; j < i; j++)
                        {
                            if (GlobalRestraints[j] == 0)
                            {
                                x[i] -= A[i, j] * x[j];
                            }
                        }
                    }
                }

                for (var i = 0; i < n; i++)
                {
                    if (GlobalRestraints[i] == 0)
                    {
                        x[i] /= diag[i];
                    }
                }

                // now back substitution is conducted on {x};  [A] is preserved

                for (var i = n - 1; i > 0; i--)
                {
                    if (GlobalRestraints[i] == 0)
                    {
                        for (var j = 0; j < i; j++)
                        {
                            if (GlobalRestraints[j] == 0)
                            {
                                x[j] -= A[i, j] * x[i];
                            }
                        }
                    }
                }

                // finally, evaluate c_r	

                for (var i = 0; i < n; i++)
                {
                    c[i] = 0.0;
                    if (GlobalRestraints[i] == 1)
                    {
                        c[i] = -b[i]; // changed from 0.0 to -b[i]; 2014-05-14
                        for (var j = 0; j < n; j++)
                        {
                            c[i] += A[i, j] * x[j];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// PRODABj -  matrix-matrix multiplication for symmetric A	      27apr01
        /// u = A * B(:,j)
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="u"></param>
        /// <param name="n"></param>
        /// <param name="j"></param>
        private void prodABj(DenseMatrix A, DenseMatrix B, DenseVector u, int j)
        {
            u.Clear();
            //for (i = 0; i < n; i++)
            //    u[i] = 0.0;
            var n = A.RowCount;
            for (var i = 0; i < n; i++)
            {
                for (var k = 0; k < n; k++)
                {
                    if (i <= k)
                    {
                        u[i] += A[i, k] * B[k, j];
                    }
                    else
                    {
                        u[i] += A[k, i] * B[k, j];
                    }
                }
            }
        }

        /// <summary>
        /// prodAB - matrix-matrix multiplication      C = A * B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="I"></param>
        /// <param name="J"></param>
        /// <param name="K"></param>
        private void prodAB(DenseMatrix A, DenseMatrix B, DenseMatrix C, int I, int J, int K)
        {
            for (var i = 0; i < I; i++)
                for (var k = 0; k < K; k++)
                {
                    C[i, k] = 0.0;
                    for (var j = 0; j < J; j++)
                    {
                        C[i, k] += A[i, j] * B[j, k];
                    }
                }
        }

        /// <summary>
        /// calculate quadratic form with inverse matrix   X' * inv(A) * Y
        /// A is n by n    X is n by m     Y is n by m
        /// </summary>
        /// <param name="X"></param>
        /// <param name="A"></param>
        /// <param name="Y"></param>
        /// <param name="Ac"></param>
        private static void xtinvAy(DenseMatrix X, DenseMatrix A, DenseMatrix Y, DenseMatrix Ac)
        {
            // double[] diag, x, y;
            // double error;
            // int  n, m;
            var ok = 1;
            var n = A.RowCount;
            var m = X.ColumnCount;
            var diag = new double[n];
            var x = new double[n];
            var y = new double[n];

            //for (int i = 0; i < n; i++)
            //    diag[i] = x[i] = 0.0;

            ldl_dcmp(A, n, diag, y, x, true, false, ref ok); //   L D L'  decomp

            for (var j = 0; j < m; j++)
            {
                for (var k = 0; k < n; k++)
                {
                    y[k] = Y[k, j];
                }

                ldl_dcmp(A, n, diag, y, x, false, true, ref ok); //   L D L'  bksbtn

                double error = 1;
                ok = 1;
                do // improve the solution
                {
                    ldl_mprove(A, n, diag, y, x, ref error, ref ok);
                } while (ok != 0);

                for (var i = 0; i < m; i++)
                {
                    Ac[i, j] = 0.0;
                    for (var k = 0; k < n; k++)
                    {
                        Ac[i, j] += X[k, i] * x[k];
                    }
                }
            }
        }

        /// <summary>
        /// xtAx - carry out matrix-matrix-matrix multiplication for symmetric A
        /// C = X' A X     C is J by J      X is N by J     A is N by N
        /// </summary>
        /// <param name="A"></param>
        /// <param name="X"></param>
        /// <param name="C"></param>
        private void xtAx(DenseMatrix A, DenseMatrix X, DenseMatrix C)
        {
            /// DenseMatrix AX;
            //int i, j, k;
            var n = A.RowCount;
            var m = X.ColumnCount;
            var AX = new DenseMatrix(n, m);
            C.Clear();

            for (var i = 0; i < n; i++)
            {
                //  use upper triangle of A
                for (var j = 0; j < m; j++)
                {
                    for (var k = 0; k < n; k++)
                    {
                        if (i <= k)
                        {
                            AX[i, j] += A[i, k] * X[k, j];
                        }
                        else
                        {
                            AX[i, j] += A[k, i] * X[k, j];
                        }
                    }
                }
            }

            // DO NOT replace with MathNet routines!!
            for (var i = 0; i < m; i++)
                for (var j = 0; j < m; j++)
                {
                    for (var k = 0; k < n; k++)
                    {
                        C[i, j] += X[k, i] * AX[k, j];
                    }
                    //  if (i != j)
                    //      C[j, i] = C[i, j];
                }

            for (var i = 0; i < m; i++) //  make  C  symmetric
                for (var j = i; j < m; j++)
                {
                    C[i, j] = C[j, i] = 0.5 * (C[i, j] + C[j, i]);
                }
        }

        /// <summary>
        /// xtAy - carry out vector-matrix-vector multiplication for symmetric A
        /// Only upper triangle contains data
        /// </summary>
        /// <param name="x"></param>
        /// <param name="A"></param>
        /// <param name="y"></param>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private double xtAy(DenseVector x, DenseMatrix A, DenseVector y, int n, DenseVector d)
        {
            var xtAy = 0.0;
            int i;
            d.Clear();
            for (i = 0; i < n; i++)
            {
                //  d = A y

                //d[i] = 0.0;
                for (var j = 0; j < n; j++)
                {
                    //  A in upper triangle only
                    if (i <= j)
                    {
                        d[i] += A[i, j] * y[j];
                    }
                    else
                    {
                        d[i] += A[j, i] * y[j];
                    }
                }
            }

            for (i = 0; i < n; i++)
            {
                xtAy += x[i] * d[i]; //  xAy = x' A y
            }

            return xtAy;
        }

#if false
    /// <summary>
    /// coordinate transform of a matrix of column 2-vectors
    /// Rr  = [ cosd(theta) -sind(theta) ; sind(theta) cosd(theta) ]*[ Rx ; Ry ];
    /// </summary>
    /// <param name="Rr"></param>
    /// <param name="R"></param>
    /// <param name="theta"></param>
    /// <param name="n"></param>
    void coord_xfrm(double[,] Rr, double[,] R, double theta, int n)
    {
        double R1, R2;
        int i;

        for (i = 0; i < n; i++)
        {
            R1 = (double)(Math.Cos(theta) * R[0, i] - Math.Sin(theta) * R[1, i]);
            R2 = (double)(Math.Sin(theta) * R[0, i] + Math.Cos(theta) * R[1, i]);
            Rr[0, i] = R1;
            Rr[1, i] = R2;
        }
    }

    /// <summary>
    /// calculate product inv(A) * B
    /// A is n by n      B is n by m	
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="n"></param>
    /// <param name="m"></param>
    /// <param name="AiB"></param>
    /// <param name="ok"></param>

    void invAB(DenseMatrix  A, DenseMatrix B, DenseMatrix AiB)
    {
        double[] diag, b, x;
        double error;
        int i, j, k,n,m, ok = 1;

        n = A.RowCount;
        m = B.ColumnCount;

        diag = new double[n ];
        x = new double[n];
        b = new double[n];

        for (i = 0; i < n; i++)
            diag[i] = x[i] = 0.0;

        ldl_dcmp(A, n, diag, b, x, true, false, ref ok);  //   L D L'  decomp
        if (ok < 0)
        {
            throw new Exception("Make sure that all six rigid body translations are restrained!");
        }

        for (j = 0; j < m; j++)
        {

            for (k = 0; k < n; k++)
                b[k] = B[k, j];
            ldl_dcmp(A, n, diag, b, x, false, true, ref ok); //   L D L'  bksbtn

            error = 1; ok = 1;
            do  // improve the solution
            {
                ldl_mprove(A, n, diag, b, x, ref error, ref ok);

            } while (ok != 0);

            for (i = 0; i < n; i++)
                AiB[i, j] = x[i];
        }
    }

    /// <summaryx>
    /// LDL_MPROVE_PM
    /// Improves a solution vector x[1..n] of the partitioned set of linear equations
    ///      [A_qq]{x_q} + [A_qr]{x_r} = {b_q}
    ///      [A_rq]{x_q} + [A_rr]{x_r} = {b_r}+{c_r}
    ///      where {b_q}, {b_r}, and {x_r} are known and
    ///      where {x_q} and {c_r} are unknown
    /// by reducing the residual r_q
    ///      A_qq r_q = {b_q} - [A_qq]{x_q+r_q} + [A_qr]{x_r}
    /// The matrix A[1..n,1..n], and the vectors b[1..n] and x[1..n]
    /// are input, as is the dimension n.   The matrix [A_qq] is the L D L'
    /// decomposition of the original system matrix, as returned by ldl_dcmp_pm().
    /// Also input is the diagonal vector, {d} of [D] of the L D L' decompositon.
    /// On output, only {x} and {c} are modified to an improved set of values.
    /// The partial right-hand-side vectors, {b_q} and {b_r}, are returned unchanged.
    /// Further, the calculations in ldl_mprove_pm do not involve b_r.
    ///
    /// usage: double **A, *d, *b, *x, rms_resid;
    /// 	int   n, ok, *restraintsNot, *r;
    ///	ldl_mprove_pm ( A, n, d, b, x, restraintsNot, r, &rms_resid, &ok );
    ///
    /// H.P. Gavin, Civil Engineering, Duke University, hpgavin@duke.edu
    /// </summaryx>
    /// <param name="A">the system matrix, and L of the L D L' decomp.</param>
    /// <param name="n">the dimension of the matrix</param>
    /// <param name="d">diagonal of D in the  L D L' - decomp'n    </param>
    /// <param name="b">the right hand side vector</param>
    /// <param name="x">part of the solution vector</param>
    /// <param name="c"> the part of the solution vector in the rhs </param>
    /// <param name="restraintsNot">restraintsNot[j]=1 if  b[j] is known; restraintsNot[j]=0 otherwise	</param>
    /// <param name="r">r[j]=1 if  x[j] is known; r[j]=0 otherwise	</param>
    /// <param name="rms_resid">root-mean-square of residual error	 </param>
    /// <param name="ok">1: >10% reduction in rms_resid; 0: not	</param>
    void ldl_mprove_pm(double[,] A, int n, double[] d, double[] b, double[] x,
        double[] c, ref double rms_resid, ref int ok)
    {
        //double sdp;  // accumulate the r.h.s. in double precision
        //double[] dx, // the residual error
          //  dc;      // update to partial r.h.s. vector, c
        double rms_resid_new = 0.0; // the RMS error of the mprvd solution

        int  pd = 0;

        var dx = new double[n ];
        var dc = new double[n ];

        for (int i = 0; i < n; i++)
            dx[i] = 0.0;

        // calculate the r.h.s. of ...
        //  [A_qq]{dx_q} = {b_q} - [A_qq]*{x_q} - [A_qr]*{x_r}
        //  {dx_r} is left unchanged at 0.0;
        for (int i = 0; i < n; i++)
        {
            if (!Restraints[i])
            {
                var sdp = b[i];
                for (int j = 0; j < n; j++)
                {
                    if (!Restraints[j])
                    {   // A_qq in upper triangle only
                        if (i <= j)
                            sdp -= A[i, j] * x[j];
                        else
                            sdp -= A[j, i] * x[j];
                    }
                }
                for (int j = 0; j < n; j++)
                    if (Restraints[j])
                        sdp -= A[i, j] * x[j];
                dx[i] = sdp;
            } // else dx[i] = 0.0; // x[i];
        }

        // solve for the residual error term, A is already factored
        ldl_dcmp_pm(A, n, d, dx, dx, dc, false, true, ref pd);

        for (int i = 0; i < n; i++)
            if (!Restraints[i])
                rms_resid_new += dx[i] * dx[i];

        rms_resid_new = Math.Sqrt(rms_resid_new / n);

        ok = 0;
        if (rms_resid_new / rms_resid < 0.90)
        { //  enough improvement
            for (int i = 0; i < n; i++)
            {  //  update the solution 2014-05-14
                if (!Restraints[i])
                    x[i] += dx[i];
                if (Restraints[i])
                    c[i] += dc[i];
            }
            rms_resid = rms_resid_new;  // return the new residual
            ok = 1;  // the solution has improved
        }
    }

    /// <summary>
    /// PSEUDO_INV - calculate the pseudo-inverse of A ,
    /// 	     Ai = inv ( A'*A + beta * trace(A'*A) * I ) * A'
    ///	     beta is a regularization factor, which should be small (1e-10)
    ///	     A is m by n
    ///	     Ai is m by n
    /// </summary>
    void PseudoInv(DenseMatrix A, ref DenseMatrix Ai, double beta)
    {
        int n = A.ColumnCount;
        int m = A.RowCount;
        DenseVector diag, b, x;
        DenseMatrix AtA, AtAi;
        double tr_AtA = 0.0, error;
        int i, j, k;
        int ok = 0;

        diag = new DenseVector(n);
        b = new DenseVector(n);
        x = new DenseVector(n);
        AtA = new DenseMatrix(n);
        AtAi = new DenseMatrix(n);

        //  ET if (beta > 1)
        // fprintf(stderr," pseudo_inv: warning beta = %lf\n", beta);

        AtA.Clear();
        diag.Clear();
        x.Clear();
        b.Clear();
        //for (i = 0; i < n; i++)
        //{
        //    diag[i] = x[i] = b[i] = 0.0;
        //    for (j = i; j < n; j++)
        //        AtA[i, j] = AtA[j, i] = 0.0;
        //}

        AtA = (DenseMatrix) A.Transpose() * A;
        //for (i = 0; i < n; i++)
        //{          // compute A' * A
        //    for (j = 0; j < n; j++)
        //    {
        //        tmp = 0.0;
        //        for (k = 0; k < m; k++)
        //            tmp += A[k, i] * A[k, j];
        //        AtA[i, j] = tmp;
        //    }
        //}
        for (i = 0; i < n; i++)                // make symmetric
            for (j = i; j < n; j++)
                AtA[i, j] = AtA[j, i] = 0.5 * (AtA[i, j] + AtA[j, i]);

        tr_AtA = AtA.Trace();
        //for (i = 0; i < n; i++)
        //    tr_AtA += AtA[i, i];   // trace of AtA

        for (i = 0; i < n; i++)
            AtA[i, i] += beta * tr_AtA;  // add beta I

        ldl_dcmp(AtA, n, diag, b, x, true, false, ref ok);  //  L D L'  decomp

        for (j = 0; j < n; j++)
        {              // compute inv(AtA)

            for (k = 0; k < n; k++)
                b[k] = 0.0;
            b[j] = 1.0;
            ldl_dcmp(AtA, n, diag, b, x, false, true, ref ok); // L D L' bksbtn

            error = 1.0; ok = 1;
            do
            {
                ldl_mprove(AtA, n, diag, b, x, ref error, ref ok);

            } while (ok != 0);


            for (k = 0; k < n; k++)
                AtAi[k, j] = x[k];  // save inv(AtA)
        }

        for (i = 0; i < n; i++)                // make symmetric
            for (j = i; j < n; j++)
                AtAi[i, j] = AtAi[j, i] = 0.5 * (AtAi[i, j] + AtAi[j, i]);

        Ai = AtAi * (DenseMatrix)A.Transpose();// compute inv(A'*A)*A'	
        //for (i = 0; i < n; i++)
        //{          // compute inv(A'*A)*A'	
        //    for (j = 0; j < m; j++)
        //    {
        //        tmp = 0.0;
        //        for (k = 0; k < n; k++)
        //            tmp += AtAi[i, k] * A[j, k];
        //        Ai[i, j] = tmp;
        //    }
        //}
    }

            /// <summary>
    /// invAXinvA -  calculate quadratic form with inverse matrix
    /// replace X with inv(A) * X * inv(A)
    /// A is n by n and symmetric   X is n by n and symmetric
    /// </summary>
    /// <param name="A"></param>
    /// <param name="X"></param>
    /// <param name="n"></param>
    void invAXinvA(double[,] A, double[,] X, int n)
    {
        double[] diag, b, x;
        double[,] Ai, XAi;
        double Aij, error;
        int i, j, k;
        int ok = 0;

        diag = new double[n ];
        x = new double[n];
        b = new double[n ];
        Ai = new double[n , n ];
        XAi = new double[n , n];

        for (i = 0; i < n; i++)
        {
            diag[i] = x[i] = b[i] = 0.0;
            for (j = 0; j < n; j++)
                XAi[i, j] = Ai[i, j] = 0.0;
        }

        ldl_dcmp(A, n, diag, b, x, true, false, ref ok); //   L D L'  decomp

        for (j = 0; j < n; j++)
        {  //  compute inv(A)

            for (k = 0; k < n; k++)
                b[k] = 0.0;
            b[j] = 1.0;
            ldl_dcmp(A, n, diag, b, x, false, true, ref ok); // L D L'  bksbtn

            error = 1; ok = 1;
            do
            {                    // improve the solution
                ldl_mprove(A, n, diag, b, x, ref error, ref ok);

            } while (ok != 0);

            for (k = 0; k< n; k++)
                Ai[j, k] = x[k];  // save inv(A)
        }

        for (i = 0; i < n; i++)   //  make symmetric
            for (j = i; j < n; j++)
                Ai[i, j] = Ai[j, i] = 0.5 * (Ai[i, j] + Ai[j, i]);

        for (i = 0; i < n; i++)
        {        //  compute X * inv(A)
            for (j = 0; j < n; j++)
            {
                Aij = 0.0;
                for (k = 0; k < n; k++)
                    Aij += X[i, k] * Ai[k, j];
                XAi[i, j] = Aij;
            }
        }

        for (i = 0; i < n; i++)
        {    //  compute inv(A) * X * inv(A)
            for (j = 0; j < n; j++)
            {
                Aij = 0.0;
                for (k = 0; k < n; k++)
                    Aij += Ai[i, k] * XAi[k, j];
                X[i, j] = Aij;
            }
        }
        for (i = 0; i < n; i++)  //  make symmetric
            for (j = i; j < n; j++)
                X[i, j] = X[j, i] = 0.5 * (X[i, j] + X[j, i]);
    }
#endif
    }
}
