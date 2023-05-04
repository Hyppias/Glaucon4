#region FileHeader
// Project: Glaucon4Test
// Filename:   TestF.cs
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
    public partial class UnitTestF : UnitTestBase
    {
        [TestMethod]
        public void TestF()
        {            
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);

            result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors)
            {
                Debug.WriteLine(e);
            }

            Assert.AreEqual(result, 0, $"Error computing {Param.InputFileName}");
            // test the force vector

         
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.MechForces.Column(0), Fmech, 7, $"{Param.InputFileName} FMech ");
            }
#if DEBUG
           
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 9, $"{Param.InputFileName} Ku");
#endif
            foreach(var mb in Glaucon.Members)
            {
                CheckVector(mb.minPeakForces, 2, $"Peak forces member {mb.Nr+1}");
            }

           


            CheckVector(gl.Glaucon.eigenFreq, Soll_freqs, 7, "Eigenfrequencies");
        }
    }
}
