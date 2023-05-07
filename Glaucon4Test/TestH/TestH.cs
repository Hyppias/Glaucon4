#region FileHeader
// Project: Glaucon4Test
// Filename:   TestH.cs
// Last write: 5/3/2023 3:38:18 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System.Diagnostics;
using System.Reflection;

namespace UnitTestGlaucon
{
    public partial class UnitTestH : UnitTestBase 
        {
         public DenseMatrix? deflection , Reactions, EndForces;
        [Test]
        public void TestH()
        {
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            Assert.That(result == 0, $"Error computing {Param.InputFileName}");

            Assert.That(295 ==  Glaucon.Members.Count, $"{Param.InputFileName} Nr of members");
            Assert.That(148 ==  Glaucon.Nodes.Count, $"{Param.InputFileName} Nr of nodes");
            Assert.That(36 ==  Glaucon.NodesRestraints.Count, $"{Param.InputFileName} Nr of restrained nodes");
            Assert.That(1 ==  Glaucon.LoadCases.Count, $"{Param.InputFileName} Nr of load cases ");
            Assert.That(0 ==  Glaucon.LoadCases[0].NodalLoads.Count, $"{Param.InputFileName} # loaded nodes");
            Assert.That(166 ==  Glaucon.LoadCases[0].UniformLoads.Count, $"{Param.InputFileName} # uniform loads");
            Assert.That(5 ==  Param.DynamicModesCount, $"{Param.InputFileName} # modes");
            // test the force vector
          
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.MechForces.Column(0), Fmech, 6, $"{Param.InputFileName} FMech ");
            }
#if DEBUG
            // no Ku known!
            //Ku.PermuteColumns(gl.Glaucon.Perm);
            //Ku.PermuteRows(gl.Glaucon.Perm);
            //CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 5, $"{Param.InputFileName} Ku");
#endif
            for (var i = 0; i < ef.RowCount; i++)
            {
                CheckVector(Glaucon.LoadCases[0].Q.Row(i), ef.Row(i), 5, $"{Param.InputFileName} Element end forces ");
            }

            // Eigenfrequencies
            CheckVector(gl.Glaucon.eigenFreq.SubVector(0, 5), sollF, 6, $"{Param.InputFileName} Eigenfrequencies: ");
            // Test the displacements vector:
            
            CheckVector(Glaucon.LoadCases[0].Displacements.Column(0), soll, 6, $"{Param.InputFileName} Displacements ");

            // test the resulting reactions vector:
            
            CheckVector2(Glaucon.LoadCases[0].Reactions.Column(0), reactions, 2,
                $"{Param.InputFileName} Load case Reactions ");
        }
    }
}
