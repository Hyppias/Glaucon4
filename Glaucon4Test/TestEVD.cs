#region FileHeader

// Solution: Glaucon
// Project: UnitTestGlaucon2
// Filename: TestEVD.cs
// Date: 2021-09-08
// Created date: 2019-12-21
// Created time:-11:32 AM
// 
// Copyright: E.H. Terwiel, 2021, the Netherlands
// 
// No part of these files may be copied in any form without written consent
// of the programmer/owner/copyrightholder.

#endregion

using System;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dbl = MathNet.Numerics.LinearAlgebra.Double;

namespace UnitTestGlaucon
{
    public partial class UnitTestGlaucon2
    {
        [TestMethod]
        public void TestEVD()
        {
            Console.WriteLine("EVD");
            double[,] A =
            {
                {665.9872e+003, 498.5706e+003, 206.0451e+003, 615.1562e+003, 122.0518e+003},
                {498.5706e+003, 894.3894e+003, 214.4199e+003, 459.3564e+003, 88.6822e+003},
                {206.0451e+003, 214.4199e+003, 516.5582e+003, 481.7492e+003, 67.5852e+003},
                {615.1562e+003, 459.3564e+003, 481.7492e+003, 702.7023e+003, 39.5670e+003},
                {122.0518e+003, 88.6822e+003, 67.5852e+003, 39.5670e+003, 153.5904e+003}
            };

            double[,] B =
            {
                {228.6695e+003, 52.2829e+003, 217.4405e+003, 81.1945e+003, 88.5512e+003},
                {52.2829e+003, 64.1871e+003, 28.4968e+003, 63.9947e+003, 9.1266e+003},
                {217.4405e+003, 28.4968e+003, 767.3295e+003, 150.4644e+003, 17.9769e+003},
                {81.1945e+003, 63.9947e+003, 150.4644e+003, 671.2022e+003, 282.6515e+003},
                {88.5512e+003, 9.1266e+003, 17.9769e+003, 282.6515e+003, 715.2125e+003}
            };

            var C = (dbl.DenseMatrix) dbl.DenseMatrix.Build.DenseOfArray(A).Inverse() *
                (dbl.DenseMatrix) dbl.DenseMatrix.Build.DenseOfArray(B);
            var E = C.Evd(Symmetricity.Symmetric);

            var ev = E.EigenValues;
            var freq = new double[ev.Count];
            for (var i = 0; i < ev.Count; i++)
            {
                freq[i] = Math.Sqrt(ev[i].Real) / (2.0 * Math.PI);
            }

            double[] Y =
            {
                0.778167123908170, 0.920180634486485, 0.935607276067470, // MathLab
                0.963915607694184, 0.985690628059698,
                1.015048284381503, 1.026313059366515, 1.063031787597951, 1.081547590133276, 1.212840409892686
            };
#if false
            // this is a matlab script:
            A = [
665.9872e+003   498.5706e+003   206.0451e+003   615.1562e+003   122.0518e+003 ;
498.5706e+003   894.3894e+003   214.4199e+003   459.3564e+003    88.6822e+003 ;
206.0451e+003   214.4199e+003   516.5582e+003   481.7492e+003    67.5852e+003 ;
615.1562e+003   459.3564e+003   481.7492e+003   702.7023e+003    39.5670e+003 ;
122.0518e+003    88.6822e+003    67.5852e+003    39.5670e+003   153.5904e+003 ];

B = [    
228.6695e+003    52.2829e+003   217.4405e+003    81.1945e+003    88.5512e+003 ;
 52.2829e+003    64.1871e+003    28.4968e+003    63.9947e+003     9.1266e+003 ;
217.4405e+003    28.4968e+003   767.3295e+003   150.4644e+003    17.9769e+003 ;
 81.1945e+003    63.9947e+003   150.4644e+003   671.2022e+003   282.6515e+003 ;
 88.5512e+003     9.1266e+003    17.9769e+003   282.6515e+003   715.2125e+003 ];

[v,D] = eig(A\B);

vSoll = [         
 731.7491e-006  -284.4655e-006   704.9697e-006    -2.5452e-003  -609.5228e-006;
 -42.5742e-006   129.9681e-006   166.6450e-006     1.2684e-003     4.4928e-003;
 535.1160e-006  -404.5401e-006  -967.0596e-006   731.2948e-006   127.8701e-006;
-894.6709e-006   470.3922e-006  -750.2641e-006  -651.5953e-006  -298.4046e-006;
-335.1719e-006    -1.1654e-003   309.6191e-006   498.0609e-006   165.2959e-006];

D = diag(D)';
D_soll =
[-83.1951e-003   288.2089e-003   953.3236e-003     3.4257e+000    14.9213e+000 ];
#endif
        }
    }
}