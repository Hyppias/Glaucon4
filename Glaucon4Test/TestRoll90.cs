#region FileHeader
// Project: Glaucon4Test
// Filename:   TestRoll90.cs
// Last write: 5/3/2023 3:38:19 PM
// Creation:   4/24/2023 12:39:31 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

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
        public void TestRoll90()
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            param.InputFileName = "TestRoll90.3dd";
            var result = ReadFile(param.InputPath + param.InputFileName);
            Assert.AreEqual(result, 0, "Error reading input file");

            var glaucon = new gl.Glaucon(ms.GetBuffer(), param);

            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            Assert.AreEqual(result, 0, "Error executing Glaucon");
            foreach (var e in gl.Glaucon.Errors)
            {
                Debug.WriteLine(e);
            }

            // Test the displacements vector:
            var soll = Vector<double>.Build.DenseOfArray(new[]
            {
                0, 0, 0, 0, 0, 0,
                double.NaN,
                0.068965517241379309,
                -0.0061041206896551761,
                -8.1388275862069015E-05,
                0,
                double.NaN
            });
            CheckVector(glaucon.LoadCases[0].Displacements.Column(0), soll, 7, $"{param.InputFileName} Displacements ");
        }
    }
}
