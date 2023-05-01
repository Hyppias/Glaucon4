#region FileHeader
// Project: Glaucon4
// Filename:   Mkl.cs
// Last write: 4/30/2023 2:29:32 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.Runtime.InteropServices;

namespace Mkl
{

    public static class NativeMethods
    {
        // This DLL must be the one built for the same processor as Glaucon was built for:
        // So either an x86 or an x64 system.
        // That goes for Both the Glaucon project and the test project.
        //const string mkl = @".\mkl_rt.dll";

        private const string Mkl = "mkl_rt.dll";

        //private const string Mkl =
        //    @"C:\Program Files (x86)\IntelSWTools\compilers_and_libraries_2019.0.117\windows\redist\intel64_win\mkl\mkl_rt.dll";

        /// <summary>
        /// http://www.netlib.org/lapack/explore-html/d9/df8/lapacke__dsygvx_8c.html
        /// </summary>
        [DllImport(Mkl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LAPACKE_dsygvx",
            ExactSpelling = true, SetLastError = false)]
        internal static extern int EigenValues2(int matrix_layout, int itype, char JOBZ, char RANGE, char UPLO,
            int N, double[] A, int LDA, double[] b, int ldb, double vl, double vu, int il, int iu,
            double abstol, ref int m, double[] w, double[] z, int ldz, int[] ifail);

        /// <summary>
        /// http://www.netlib.org/lapack/explore-html/d5/d2e/dsygv_8f.html
        /// </summary>
        [DllImport(Mkl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LAPACKE_dsygv", ExactSpelling = true,
            SetLastError = false)]
        internal static extern int EigenValues(int matrix_layout, int ITYPE, int JOBZ, int UPLO,
            int N, double[] A, int LDA, double[] B, int LDB, double[] W);

        /// <summary>
        /// http://www.netlib.org/lapack/explore-html/d5/d2e/dsygv_8f.html
        /// </summary>
        [DllImport(Mkl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LAPACKE_dlamch",
            ExactSpelling = true, SetLastError = false)]
        internal static extern double MachinePrecision(char c); // c = 'E' for machine precision

#if false
        [DllImport(mkl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "dfeast_sygv", ExactSpelling = true)]
        public static  extern void EigenValuesFEAST(ref int uplo, ref int n, double[] a, ref int lda, /* stiffness*/
           double[] b, ref int ldb, /* mass */
           int[] fpm, ref double epsout, ref int loop,
           ref double emin, ref double emax,
           ref int m0, double[] e, double[] x, ref int m, double[] res, ref int info);

        [DllImport(mkl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "feastinit", ExactSpelling = true)]
        public static  extern void Init(int[] fpm);
#endif
        [DllImport(Mkl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LAPACKE_dgesvx",
            ExactSpelling = true)]
        internal static extern int Solve(int matrix_layout, char fact, char trans, int n,
            int nrhs, [In][Out] double[] a, int lda, [Out] double[] af, int ldaf,
            [Out] int[] ipiv, ref char equed, [Out] double[] r, [Out] double[] c, [In][Out] double[] b, int ldb,
            [Out] double[] x, int ldx, ref double rcond, [Out] double[] ferr, [Out] double[] berr, ref double rpivot);
    }
}
