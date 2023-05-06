#region FileHeader
// Project: Glaucon4Test
// Filename:   TestC.cs
// Last write: 5/3/2023 3:38:17 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System.Collections.Generic;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class UnitTestC : UnitTestBase
    {
         public DenseMatrix? deflection , Reactions, EndForces;
        [Test]
        public void TestC()
        {
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);

            Assert.That(result== 0, $"Error computing {Param.InputFileName}");
            // test the force vector

            //for (int i = 0; i < Glaucon.LoadCases.Length; i++)
            CheckVector(Glaucon.LoadCases[0].MechForces.Column(0), Fmech, 6, $"{Param.InputFileName} FMech ");
#if DEBUG
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 6, $"{Param.InputFileName} Ku");
#endif
            CheckVector(Glaucon.LoadCases[0].Displacements.Column(0), soll, 3, $"{Param.InputFileName} Displacements ");

            // test the resulting Reactions vector:           

            CheckVector(Glaucon.LoadCases[0].Reactions.Column(0), reactionsSoll, 3,
                $"{Param.InputFileName} Reactions ");

            CheckVector(gl.Glaucon.eigenFreq, Soll_freqs, 4, $"{Param.InputFileName} Eigenfrequencies");
        }
    }
}
