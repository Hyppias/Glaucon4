#region FileHeader
// Project: Glaucon4Test
// Filename:   TestJ.cs
// Last write: 5/3/2023 3:38:19 PM
// Creation:   4/24/2023 12:39:31 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class UnitTestJ : UnitTestBase
    {
        int result;
        [Test]
        public void TestJ()
        {           
            
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            
            // won't execute the following, because the expected exception will occur first
            var ex  = Assert.Catch<InvalidOperationException>(() => GetGlaucon());
            Assert.Throws<InvalidOperationException>(() => GetGlaucon());
            
            //Assert.AreEqual(result, 0, $"Error executing {Param.InputFileName}");
            foreach (var e in gl.Glaucon.Errors)
            {
                Debug.WriteLine(e);
            }
            // the test will succeed, because the expected exception will occur
        }

        private void GetGlaucon()
        {
           result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
        }
    }
}
