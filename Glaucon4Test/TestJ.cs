#region FileHeader
// Project: Glaucon4Test
// Filename:   TestJ.cs
// Last write: 5/3/2023 2:00:25 PM
// Creation:   4/24/2023 12:39:31 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

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
            var TestObject = new TestJobject();
            var param = TestObject.Param;
            var glaucon = TestObject.Glaucon;
            
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);

            // won't execute the following, because the expected exception will occur first
            var result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            Assert.AreEqual(result, 0, $"Error executing {param.InputFileName}");
            foreach (var e in gl.Glaucon.Errors)
            {
                Debug.WriteLine(e);
            }
            // the test will succeed, because the expected exception will occur
        }
    }
}
