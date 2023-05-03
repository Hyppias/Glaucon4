#region FileHeader
// Project: Glaucon4Test
// Filename:   TestSolver.cs
// Last write: 4/22/2023 4:14:35 PM
// Creation:   4/24/2023 12:39:31 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

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
