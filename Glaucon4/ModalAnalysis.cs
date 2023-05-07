#region FileHeader
// Project: Glaucon4
// Filename:   ModalAnalysis.cs
// Last write: 4/30/2023 2:29:34 PM
// Creation:   4/24/2023 12:01:05 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

#define EIGENVALUES_ALL

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using Mkl;
using dm = MathNet.Numerics.LinearAlgebra.Double.DenseMatrixExtensions;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Numerics;
using Newtonsoft.Json;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        private const int itype = 1;

        private DenseVector omega2; // eigenvalues.
        private char UpOnly = 'U';

        [XmlElement("Iterations"), JsonProperty("Iterations")]
        [Description("Eigenfrequencies iterations")]
        private int iter { get; set; } // number of iterations	

        [XmlMatrix("Eigenvectors")]
        [Description("Eigenvectors")]
        public DenseMatrix Eigenvector { get; set; }

        [XmlVector("MPF"), JsonProperty("MPF")]
        [Description("Modal Participation Factor")]
        public DenseMatrix ModParticFactor { get; set; }

        private void PrepK_M()
        {
            TraceK = TraceM = 0.0;
            for (var j = 0; j < DoF; j++)
            //  compute traceK and traceM
            {
                if (GlobalRestraints[j] == 0)
                {
                    TraceK += K[j, j];
                    TraceM += M[j, j];
                }
            }

            for (var i = 0; i < DoF; i++)
            //  modify K and M for reactions
            {
                if (GlobalRestraints[i] == 1)
                {
                    // apply reactions to upper triangle
                    K[i, i] = TraceK * 1e4;
                    M[i, i] = TraceM;
                    for (var j = i + 1; j < DoF; j++)
                    {
                        K[j, i] = K[i, j] = M[j, i] = M[i, j] = 0.0;
                    }
                }
            }

            var ii = M.Diagonal().MinimumIndex();
            if (M[ii, ii] <= 0)
            {
                throw new Exception($"error: Non pos-def mass matrix: M[{ii},{ii}] = {M[ii, ii]}");
            }
        }

        /// <summary>
        /// you may want to execute a modal Analysis only,
        /// That is, get the eigenfrequencies of the structure.
        /// </summary>
        private void ModalAnalysis()
        {
            StartClock(2);
            var jobz = 'N'; // if JOBZ = V, then also the eigenvectors in Z
            const int LAPACK_ROW_MAJOR = 101;
            //const int LAPACK_COL_MAJOR = 102;

            try
            {
                M = new DenseMatrix(DoF);
                AssembleSystemMassMatrix();

                switch (Param.ModalMethod)
                {
                    case EVG: // = 7
                        // see https://github.com/wo80/mathnet-extensions/blob/master/src/Numerics/LinearAlgebra/Double/DenseMatrixExtensions.cs

                        DenseMatrix EigenVectors =new DenseMatrix(DoF); // Bathe ?
                            
                        var eigenValues = dm.GeneralizedEigenvalues(K,M,EigenVectors);
                        
                        break;
                    case MKL:
                        omega2 = new DenseVector(DoF); // square of omega

                        //AssembleSystemStiffnessMatrix(K, Members, !ReArrange);
                        //PrepK_M();
                        var C = (DenseMatrix)K.Inverse() * M;
                        var E = C.Evd(Symmetricity.Symmetric);
                        var ev = E.EigenValues;
                        for (var i = 0; i < ev.Count; i++)
                        {
                            if (ev[i].Real >= 0)
                            {
                                omega2[i] = ev[i].Real;
                            }
                        }

                        break;
                    case STODOLA:
                        // Bathe pg. 679: q = min(2p, p+8)
                        var modesBathe = Math.Min(2 * Param.DynamicModesCount, Param.DynamicModesCount + 8);
                        //AssembleSystemStiffnessMatrix(K, Members, !ReArrange);
                        omega2 = new DenseVector(modesBathe);
                        Eigenvector = new DenseMatrix(DoF, modesBathe);
                        var ok = 0;
                        var abstol = 1E-6;
                        PrepK_M();
                        Stodola(omega2.AsArray(), Eigenvector, ref ok, abstol);
                        break;
                    case SUBSPACE:
                        // Bathe pg. 679: q = min(2p, p+8)
                        modesBathe = Math.Min(2 * Param.DynamicModesCount, Param.DynamicModesCount + 8);

                        omega2 = new DenseVector(modesBathe);
                        Eigenvector = new DenseMatrix(DoF, modesBathe);
                        ok = 0;
                        abstol = 1E-6;
                        PrepK_M();
                        WriteMatrix(MODAL, "E", "K2.dat", "K2", K);
                        WriteMatrix(MODAL, "E", "MjustBuilt.dat", "M2", M);
                        Subspace(omega2.AsArray(), Eigenvector, ref ok, abstol);
                        break;
#if false //  FEAST algorithm
                case ModalMethod.FEAST:
                var fpm = new int[128];

                double emin = Param.minEigenvalue;
                double emax = Glaucon.Param.maxEigenvalue;
                int m0 = DoF;// Nr of freq. found in interval

                Glaucon.Param.FrequenciesFound = 1;
                int loop = 0; // ignored on input

                double epsout = 0;
                double[] x = new double[DoF* Glaucon.Param.FrequenciesFound * 20];
                double[] res = new double[DoF];

                NativeMethods.Init(fpm);
#if false // FPM vector of parameters
                    fpm[0] = 1;     // Specifies whether Extended Eigensolver routines print runtime status.
                                //  0:  Extended Eigensolver routines do not generate runtime messages at all.
                                //  1: Extended Eigensolver routines print runtime status to the screen.
                fpm[1] = 8;     // The number of contour points Ne = 8 (see the description of FEAST algorithm).
                                //      Must be one of {3,4,5,6,8,10,12,16,20,24,32,40,48}.
                fpm[2] = 12;    // Error trace double precision stopping criteria ε (ε = 10^-fpm[2]) .
                fpm[3] = 20;    // Maximum number of Extended Eigensolver refinement loops allowed.
                                //  If no convergence is reached within fpm[3] refinement loops,
                                //   Extended Eigensolver routines return info=2.
                fpm[4] = 0;      // User initial subspace. If fpm[4]=0 then Extended Eigensolver routines
                                    //  generate initial subspace, if fpm[4]=1 the user supplied initial
                                    //  subspace is used.
                fpm[5] = 0;     // Extended Eigensolver stopping test. 0 or 1. See
                                // https://software.intel.com/en-us/mkl-developer-reference-c-extended-eigensolver-input-parameters
                                //  fpm[6] = 5;     // Error trace single precision stopping criteria (10^fpm[6]) .
                fpm[13] = 0;
                fpm[26] = 0;    // Specifies whether Extended Eigensolver routines check input
                                //  matrices (applies to CSR format only).
                                //  0 : Extended Eigensolver routines do not check input matrices.
                                //  1 : Extended Eigensolver routines check input matrices.
                fpm[27] = 0;    // Check if matrix B is positive definite.
                                //  Set fpm[27] = 1 to check if B is positive definite.
                fpm[63] =
0;    // Use the Intel MKL PARDISO solver with the user-defined PARDISO iparm array settings.
                                // 0 : Extended Eigensolver routines use the Intel MKL PARDISO default
                                //      iparm settings defined by calling the pardisoinit subroutine.
                                // 1 : The values from fpm[64] to fpm[127] correspond to iparm[0]
                                //      to iparm[63] respectively according to the formula
                                //      fpm[64 + i] = iparm[i] for i = 0, 1, .., 63.
#endif
                NativeMethods.EigenValuesFEAST(ref Uplo, ref DoF, K.AsColumnMajorArray(), ref DoF,
                    M.AsColumnMajorArray(), ref DoF,
                    fpm, ref epsout, ref loop,
                    ref emin, ref Glaucon.Param.maxEigenvalue, ref m0, freq.AsArray(), x,
                    ref Glaucon.Param.FrequenciesFound, res, ref info);
                switch (info)
                {
                    case 0:
                        freq = (DenseVector) freq.SubVector(0, Glaucon.Param.FrequenciesFound);
                        Array.Sort(freq.ToArray());
                        break;
                    case 202:
                        throw new ArgumentOutOfRangeException("Problem with size of the system n (n≤0)");
                    case 201:
                        throw new ArgumentOutOfRangeException("Problem with size of initial subspace m0 (m0≤0 or m0>n)");
                    case 200:
                        throw new ArgumentOutOfRangeException("Problem with EMIN, EMAX or EMID,r");
                    case 6:
                        Errors.Add("FEAST converges but subspace is not bi-orthonormal");
                        break;
                    case 4:
                        Errors.Add("Successful return of only the computed subspace after call with fpm[13] = 1");
                        break;
                    case 3:
                        Errors.Add("initial guess of subspace M0 is too small.");
                        break;
                    case 2:
                        Errors.Add($"No Convergence (number of iteration loops > {fpm[3]})");
                        break;
                    case 1:
                        Errors.Add("No eigenvalue found in the search interval. See web page (remarks at the bottom)" +
                            Environment.NewLine +
                            "for further details" + Environment.NewLine +
                            "https://software.intel.com/en-us/mkl-developer-reference-c-extended-eigensolver-output-details#GUID-E1DB444D-B362-4DBF-A1DF-DA68F7FB7019");
                        break;
                    case -1:
                        throw new ArgumentOutOfRangeException("Internal error for allocation memory.");
                    case -2:
                        throw new ArgumentOutOfRangeException("Internal error of the inner system solver. Possible reasons:" +
                            Environment.NewLine +
                            "not enough memory for inner linear system solver or inconsistent input.");
                    case -3:
                        throw new ArgumentOutOfRangeException("Internal error of the reduced eigenvalue solver" +
                            Environment.NewLine +
                            "Possible cause: matrix B may not be positive definite.It can be " + Environment.NewLine +
                            "checked by setting fpm[27] = 1 before calling an Extended Eigensolver " + Environment.NewLine +
                            "routine, or by using LAPACK routines.");
                    case -4:
                        throw new ArgumentOutOfRangeException("Matrix B is not positive definite.");
                    default:
                        if (info < -100)
                            throw new ArgumentOutOfRangeException($"Error with the {-info - 100}-th argument" +
                                " of the Extended Eigensolver interface.");
                        else
                            if (info > 100)
                            throw new ArgumentOutOfRangeException($"Problem with {info - 100}-th value of the" +
                                Environment.NewLine +
                                $"input Extended Eigensolver parameter(fpm[{info - 101}])." + Environment.NewLine +
                                "Only the parameters in use are checked.");
                        break;
                }

                break;
#endif
                    case RANGE:
                        //AssembleSystemStiffnessMatrix(K, Members, !ReArrange);
                        int il = 1;

                        var p = NativeMethods.MachinePrecision('S');
                        abstol = 2d * p;
                        //ul = Param.DynamicModesCount;
                        Param.FrequenciesFound = Param.DynamicModesCount - il + 1;

                        omega2 = new DenseVector(DoF);
                        var Range = 'I'; // A = all, I = 0 -ith value, V = interval
                        DenseMatrix z;
                        var ifail = new int[DoF];
                        z = jobz == 'V' ? new DenseMatrix(DoF) : new DenseMatrix(1, DoF);
                        //uplo(); // only upper triangles of K and M
                        var info = NativeMethods.EigenValues2(LAPACK_ROW_MAJOR, itype, jobz, Range, UpOnly, DoF,
                            K.AsColumnMajorArray(),
                            DoF, M.AsColumnMajorArray(), DoF, Param.MinEigenvalue, Param.MaxEigenvalue, il, Param.DynamicModesCount, abstol,
                            ref Param.FrequenciesFound, omega2.AsArray(), z.AsColumnMajorArray(),
                            z.ColumnCount, ifail);
                        if (info < 0)
                        {
                            var parNames = new[]
                            {
                            "LAPACK_ROW_MAJOR", "ITYPE", "JOBZ", "Range", "UPLO", "DoF", "K", "DoF-2", "M",
                            "DoF-3", "Min. eigenvalue", "Max. eigenvalue", "il", "ul", "abstol",
                            "freq. found", "freq", "z", "DoF-4", "ifail"
                        };
                            throw new Exception($"Eigenvalues2: parameter {parNames[-info - 1]} was wrong.");
                        }

                        if (info >= DoF && info <= 2 * DoF)
                        {
                            throw new Exception("Error calculating eigenvalues:" + Environment.NewLine +
                                $"the leading minor of order {info - DoF} of the mass matrix is not positive-definite. ");
                        }
                        else if (info > 0 && info < DoF)
                        {
                            throw new Exception($"Error calculating eigenvalues: No convergence. Error =  {info}");
                        }

                        break;
                    default:
                        AssembleSystemStiffnessMatrix(Members, !ReArrange);
                        omega2 = new DenseVector(DoF);
                        info = NativeMethods.EigenValues(LAPACK_ROW_MAJOR, itype, jobz, UpOnly, DoF,
                            K.AsColumnMajorArray(), DoF, M.AsColumnMajorArray(), DoF, omega2.AsArray());

                        if (info < 0)
                        {
                            var parNames = new[]
                            {
                            "LAPACK_ROW_MAJOR", "ITYPE", "JOBZ", "UPLO", "DoF", "K", "DoF-2", "M",
                            "DoF-3", "freq"
                        };
                            throw new Exception(
                                $"Error calculating eigenvalues: error in argument {parNames[-info - 1]}");
                        }
                        else if (info >= DoF && info <= 2 * DoF)
                        {
                            throw new Exception("Error calculating eigenvalues:" + Environment.NewLine +
                                "The mass matrix is not positive definite. Check masses.");
                        }
                        else if (info > 0 && info < DoF)
                        {
                            throw new Exception($"Error calculating eigenvalues: No convergence. Error =  {info}");
                        }

                        Param.FrequenciesFound = DoF;
                        if (jobz == 'V')
                        {
                            z = K;
                        }

                        break;
                } // end switch Param.ModalMethod

                Array.Sort(omega2.AsArray());
                // square root: rad/sec
                eigenFreq = (DenseVector)omega2.PointwiseSqrt() / (2d * Math.PI); // bpm
                // write modal results:
                WriteModalResults();
            } // end try
            catch (BadImageFormatException bie)
            {
                Errors.Add(bie.Message);
                Errors.Add("Probably an x86/x64 confusion");
                throw bie;
            }
            catch (SEHException sehex)
            {
                Errors.Add(sehex.Message);
                throw sehex;
            }
            catch (Exception ex)
            {
                Errors.Add(ex.Message);
                throw ex;
            }
            finally
            {
                StopClock(2, "Modal analysis");
            }
        } // end ModalAnalysis

        private void WriteModalResults()
        {
            var ms = new DenseMatrix(DoF, 3);

            for (var i = 0; i < DoF; i++)
                for (var k = 0; k < 3; k++)
                    for (var j = k; j < DoF; j += 6)
                    {
                        ms[i, k] += M[i, j];
                    }

            var numModes = Math.Min(Param.DynamicModesCount, DoF - MinRestraints);
            ModParticFactor = new DenseMatrix(numModes, 3);
            for (var m = 0; m < numModes; m++)
                for (var k = 0; k < 3; k++)
                    for (var i = 0; i < DoF; i++)
                    {
                        ModParticFactor[m, k] += Eigenvector[i, m] * ms[i, k];
                    }
        }
    }
}
