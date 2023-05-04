#region FileHeader
// Project: Glaucon4Test
// Filename:   TestE.cs
// Last write: 5/3/2023 3:38:17 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Double;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestE : UnitTestBase
    {
        [TestMethod]
        public void TestE()
        {
           
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            Param.Analyze = false;
            result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);

            foreach (var e in gl.Glaucon.Errors)
            {
                Console.WriteLine(e);
            }

            Assert.AreEqual(result, 0, $"Error computing {Param.InputFileName}");

#if DEBUG
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 10, $"{Param.InputFileName} Ku");
#endif
            // test the force vector
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.MechForces.Column(0), Fmech, 10, $"{Param.InputFileName} FMech ");
            }

            CheckVector(gl.Glaucon.eigenFreq.SubVector(0, sollEig.Count), sollEig, 2,
                $"{Param.InputFileName} EigenFrequencies ");

            // test the resulting Reactions vector:
           

            CheckVector(Glaucon.LoadCases[0].Reactions.Column(0), _reactions, 4,
                $"{Param.InputFileName} Reactions ");

            // Test the displacements vector:

            CheckVector(Glaucon.LoadCases[0].Displacements.Column(0), soll, 2, $"{Param.InputFileName} Displacements ");

            CheckMatrix(Glaucon.LoadCases[0].MinMaxForce, _minmax, 2,
                $"{Param.InputFileName} MinMax ");
        }
    }
}