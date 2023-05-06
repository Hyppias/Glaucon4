#region FileHeader
// Project: Glaucon4Test
// Filename:   TestB.cs
// Last write: 5/3/2023 3:38:17 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System.Collections.Generic;
using System.Reflection;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class UnitTestB : UnitTestBase
    {
         public DenseMatrix? deflection , Reactions, EndForces;
        [Test]
        public void TestB()
        {

            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            Assert.That(0== result, $"{Param.InputFileName} Exit code Glaucon");
            Assert.That(4== Glaucon.Members.Count, $"{Param.InputFileName} Nr of members");
            Assert.That(5== Glaucon.Nodes.Count, $"{Param.InputFileName} Nr of nodes");
            Assert.That(4== Glaucon.NodeRestraints.Count, $"{Param.InputFileName} Nr of restrained nodes");
            Assert.That(3== Glaucon.LoadCases.Count, $"{Param.InputFileName} Nr of load cases");

            for (var i = 0; i < Glaucon.LoadCases.Count; i++)
            {
                var lc1 = Glaucon.LoadCases[i];
                Assert.IsNotNull(lc1, $"Load case {i + 1} set to NULL");
                Assert.That(n[i, 0]== lc1.NodalLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of loaded nodes");
                Assert.That(n[i, 1]== lc1.UniformLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of uniform loads");
                Assert.That(n[i, 2]== lc1.TrapLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of Trap loads");
                Assert.That(n[i, 3]== lc1.IntPointLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of Conc. loads");
                Assert.That(n[i, 4]==lc1.TempLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of temperature loads");
                Assert.That(n[i, 5]== lc1.PrescrDisplacements.Count, $"{Param.InputFileName} Load case {i + 1} Nr of pescr. displ");
            }

#if DEBUG
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 3,  $"{Param.InputFileName} Ku");
            Assert.That(Param.DynamicModesCount==6, $"Nr of dyn.modes nM");
#endif
            var mbr = Glaucon.Members[0];

            Matrix<double> m = mbr.ConsistentMassMatrix();
            //CheckMatrix(m, sollCons, 5, $"{Param.InputFileName} ConsistentMassMatrix");


            mbr = Glaucon.Members[0];

            m = mbr.LumpedMassMatrix();
            CheckMatrix(m, sollLump, 6, $"{Param.InputFileName} LumpedMassMatrix");

            //var g = Matrix<double>.Build.Dense(12, 12);
            for (var i = 0; i < Glaucon.Members.Count; i++)
            {
                CheckMatrix(Glaucon.Members[i].Gamma.SubMatrix(0, 3, 0, 3), sollg[i], 7,
                    $"{Param.InputFileName} Gamma for member {i + 1}");
            }
            // check end forces
            foreach (var ld in Glaucon.LoadCases)
            {
                var Q1 = ld.Q;
                foreach (var mb in Glaucon.Members)
                {
                    var mbrQ = Q1.Row(mb.Nr);
                    CheckVector(mbrQ, sollEndForces[mb.Nr].Row(mb.Nr), 2,
                      $"{Param.InputFileName} EndForces for member {mb.Nr + 1}");

                }
            }

            //soll = soll.Transpose();
            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.Reactions.Column(0), sollReactions.Transpose().Column(lc.Nr), 2,
                    $"{Param.InputFileName} Reactions for loadcase {lc.Nr + 1}:");
            }

            foreach (var lc in Glaucon.LoadCases)
            {
                CheckVector(lc.Displacements.Column(0).SubVector(0, 6), soll5.Transpose().Column(lc.Nr),
                    2, $"{Param.InputFileName} Displacements loadcase {lc.Nr + 1}");
            }

            // Test frequencies:
            var sollv = (DenseVector)Vector<double>.Build.DenseOfArray(new[]
                {18.807943, 19.105451, 19.690439, 31.711570, 35.159165, 42.248953});
            //Assert.That( 3,gl.Glaucon.Param.FrequenciesFound,  "Eigenfrequensies found: ");

            CheckVector(gl.Glaucon.eigenFreq, sollv, 3, $"{Param.InputFileName} EigenFrequencies ");

            foreach (var ld in Glaucon.LoadCases)
            {
                foreach (var mb in Glaucon.Members)
                {
                    CheckVector(mb.maxPeakDisplacements, sollPeakDispl[ld.Nr].Row(mb.Nr), 3,
                        $"{Param.InputFileName} Maximum Peak Displacements {mb.Nr + 1}");
                    CheckVector(mb.minPeakDisplacements, sollPeakDispl[ld.Nr].Row(mb.Nr), 3,
                        $"{Param.InputFileName} Minimum Peak Displacements member {mb.Nr + 1}");
                }
            }

            foreach (var ld in Glaucon.LoadCases)
            {
                foreach (var mb in Glaucon.Members)
                {
                    CheckVector(mb.maxPeakForces, sollPeakDispl[ld.Nr].Row(mb.Nr), 4, $"{Param.InputFileName} Maximum Peak forces  member {mb.Nr + 1}");
                    CheckVector(mb.minPeakForces, sollEndForces[ld.Nr].Row(mb.Nr), 4, $"{Param.InputFileName} Minimum Peak forces , member {mb.Nr + 1}");
                }
            }


            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }
    }
}
