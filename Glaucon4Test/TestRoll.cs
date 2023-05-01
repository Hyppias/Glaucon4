#region FileHeader

// Solution: Glaucon
// Project: UnitTestGlaucon2
// Filename: TestRoll.cs
// Date: 2021-09-09
// Created date: 2019-12-15
// Created time:-7:18 PM
// 
// Copyright: E.H. Terwiel, 2021, the Netherlands
// 
// No part of these files may be copied in any form without written consent
// of the programmer/owner/copyrightholder.

#endregion

using System.Diagnostics;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestGlaucon2
    {
        [TestMethod]
        public void TestRoll()
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            param.InputFileName = "TestRoll.3dd";
            var result = ReadFile(param.InputPath + param.InputFileName);
            Assert.AreEqual(result, 0, "Error reading input file");

            var glaucon = new gl.Glaucon(ms.GetBuffer(), param);

            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            Assert.AreEqual(result, 0, "Error setting up Glaucon");
            foreach (var e in gl.Glaucon.Errors)
            {
                Debug.WriteLine(e);
            }

            // Test the displacements vector:
            var soll = Vector<double>.Build.DenseOfArray(new[]
            {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0.068965517241379309,
                -0.012208241379310352,
                -0.00016277655172413803,
                0, 0
            });
            CheckVector(glaucon.LoadCases[0].Displacements.Column(0), soll, 6, $"{param.InputFileName} Displacements ");
        }
    }
}