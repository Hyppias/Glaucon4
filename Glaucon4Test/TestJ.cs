#region FileHeader

// Solution: Glaucon
// Project: UnitTestGlaucon2
// Filename: TestJ.cs
// Date: 2021-09-09
// Created date: 2019-12-15
// Created time:-7:38 PM
// 
// Copyright: E.H. Terwiel, 2021, the Netherlands
// 
// No part of these files may be copied in any form without written consent
// of the programmer/owner/copyrightholder.

#endregion

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestGlaucon2
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The construction is not restrained.")]
        public void TestJ()
        {
            // this construction is not restrained and therefor cannot be processed.
            param.InputFileName = "exJ.3dd";
            var result = ReadFile(param.InputPath + param.InputFileName);
            Assert.AreEqual(result, 0, $"Error reading {param.InputFileName}");

            var glaucon = new gl.Glaucon(ms.GetBuffer(), param);

            // won't execute the following, because the expected exception will occur first
            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            Assert.AreEqual(result, 0, $"Error executing {param.InputFileName}");
            foreach (var e in gl.Glaucon.Errors)
            {
                Debug.WriteLine(e);
            }
            // the test will succeed, because the expected exception will occur
        }
    }
}