#region FileHeader
// Project: Glaucon4Test
// Filename:   TestCoCa1.cs
// Last write: 5/3/2023 3:38:17 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Double;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestCoCa1 : UnitTestBase
    {
        [Test]
        public void TestCoCa1()
        {            
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);

            Assert.AreEqual(result, 0, $"Error computing {Param.InputFileName}");
            // test the force vector

#if DEBUG
            
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            //CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 5,$"{Param.InputFileName} Ku");
#endif
            // Test the displacements vector:
          
            CheckVector(Glaucon.LoadCases[0].Displacements.Column(0), soll, 6, $"{Param.InputFileName} Displacements ");
            // test the resulting Reactions vector:
            

            CheckVector(Glaucon.LoadCases[0].Reactions.Column(0), _reactions, 6, $"{Param.InputFileName} Reactions ");
           
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.MechForces.Column(0), Fmech, 4, $"{Param.InputFileName} FMech ");
            }

            // Test the member end forces:
           
        }
    }
}
