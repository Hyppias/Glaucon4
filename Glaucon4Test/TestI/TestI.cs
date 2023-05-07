#region FileHeader
// Project: Glaucon4Test
// Filename:   TestI.cs
// Last write: 5/3/2023 3:38:19 PM
// Creation:   4/24/2023 12:39:31 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class UnitTestI : UnitTestBase
    {
         public DenseMatrix? deflection , Reactions, EndForces;
        [Test]
        public void TestI()
        {
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);           

            Assert.That(24 == Glaucon.Members.Count, $"{Param.InputFileName} Nr of members");
            Assert.That(15 == Glaucon.Nodes.Count, $"{Param.InputFileName} Nr of nodes");
            Assert.That(3 == Glaucon.NodesRestraints.Count, $"{Param.InputFileName} Nr of restrained nodes");
            Assert.That(1 == Glaucon.LoadCases.Count, $"{Param.InputFileName} Nr of load cases.LoadCases.Count");
            Assert.That(1 == Glaucon.LoadCases[0].NodalLoads.Count, $"{Param.InputFileName} # loaded nodes");
            Assert.That(12 == Glaucon.LoadCases[0].UniformLoads.Count, $"{Param.InputFileName} # uniform loads");
            Assert.That(4 == Param.DynamicModesCount, $"{Param.InputFileName} # modes");
            Assert.That(result == 0, $"Error computing {Param.InputFileName}")  ;  // test the force vector
            
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.MechForces.Column(0), Fmech, 4, $"{Param.InputFileName} FMech ");
            }
#if DEBUG
            
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 6, $"{Param.InputFileName} Ku");
#endif


            CheckVector(gl.Glaucon.eigenFreq, sollF, 6, $"{Param.InputFileName} Eigenfrequencies: ");


            CheckVector(Glaucon.LoadCases[0].Reactions.Column(0), _reactions, 3, $"{Param.InputFileName} Reactions ");

            // Test the displacements vector:            
            CheckVector(Glaucon.LoadCases[0].Displacements.Column(0), soll, 4, $"{Param.InputFileName} Displacements ");

            // Test the member end forces:
            for (var i = 0; i < ef.RowCount; i++)
            {
                CheckVector(Glaucon.LoadCases[0].Q.Row(i), ef.Row(i), 5, $"{Param.InputFileName} Element end forces");
            }
        }
    }
}
