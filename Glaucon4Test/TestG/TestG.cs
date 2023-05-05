#region FileHeader
// Project: Glaucon4Test
// Filename:   TestG.cs
// Last write: 5/3/2023 3:38:17 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System.Diagnostics;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class UnitTestG : UnitTestBase
    {
        [Test]
        public void TestG()
        {
            
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);

            Assert.AreEqual(24, Glaucon.Members.Count, $"{Param.InputFileName} Nr of members");
            Assert.AreEqual(15, Glaucon.Nodes.Count, $"{Param.InputFileName} Nr of nodes");
            Assert.AreEqual(3, Glaucon.NodeRestraints.Count, $"{Param.InputFileName} Nr of restrained nodes");
            Assert.AreEqual(1, Glaucon.LoadCases.Count, $"{Param.InputFileName} Nr of load cases LoadCases.Count");
            Assert.AreEqual(1, Glaucon.LoadCases[0].NodalLoads.Count, $"{Param.InputFileName} # loaded nodes");
            Assert.AreEqual(12, Glaucon.LoadCases[0].UniformLoads.Count, $"{Param.InputFileName} # uniform loads");
            Assert.AreEqual(4, Param.DynamicModesCount, $"{Param.InputFileName} # modes");
            Assert.AreEqual(result, 0, $"Error computing {Param.InputFileName}");
            // test the force vector
          
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.MechForces.Column(0), Fmech, 7, $"{Param.InputFileName} FMech ");
            }
#if DEBUG
            
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 6, $"{Param.InputFileName} Ku");
#endif
            // Test the displacements vector:
            
            CheckVector(Glaucon.LoadCases[0].Displacements.Column(0), sollDispl, 4,
                $"{Param.InputFileName} Displacements ");
            // test the resulting Reactions vector:

            CheckVector(Glaucon.LoadCases[0].Reactions.Column(0), _reactions, 7, $"{Param.InputFileName} Reactions ");

            // Eigenfrequencies           
            CheckVector(gl.Glaucon.eigenFreq, sollF, 6, $"{Param.InputFileName} Eigenfrequencies: ");

            // only ONE load case
            // Test the member end forces:            
            for (var i = 0; i < ef.RowCount; i++)
            {
                CheckVector(Glaucon.LoadCases[0].Q.Row(i), ef.Row(i), 5, $"{Param.InputFileName} Element end forces");
            }
        }
    }
}
