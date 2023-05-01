#region FileHeader

// Solution: Glaucon
// Project: UnitTestGlaucon2
// Filename: TestSolver.cs
// Date: 2021-09-09
// Created date: 2019-12-17
// Created time:-2:59 PM
// 
// Copyright: E.H. Terwiel, 2021, the Netherlands
// 
// No part of these files may be copied in any form without written consent
// of the programmer/owner/copyrightholder.

#endregion

using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGlaucon
{
    public partial class UnitTestGlaucon2
    {
        [TestMethod]
        public void CanSolveForRandomMatrix()
        {
            var order = 10;
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixB = Matrix<double>.Build.Random(order, order, 1);

            var monitor = new Iterator<double>(
                new IterationCountStopCriterion<double>(1000),
                new ResidualStopCriterion<double>(1e-10));
            var sw = Stopwatch.StartNew();

            var solver = new MlkBiCgStab();
            var matrixX = matrixA.SolveIterative(matrixB, solver, monitor);
            sw.Stop();
            Debug.WriteLine($"solver took {sw.ElapsedMilliseconds} msec.");
            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-7);
                }
            }
        }
    }
}