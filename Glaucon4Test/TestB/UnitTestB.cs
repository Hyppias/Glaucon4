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
using System.Diagnostics;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Double;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestB : UnitTestBase
    {
        [TestMethod]
        public void TestB()
        {
           
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            Assert.AreEqual(0, result, $"{Param.InputFileName} Exit code Glaucon");
            Assert.AreEqual(4, Glaucon.Members.Count, $"{Param.InputFileName} Nr of members");
            Assert.AreEqual(5, Glaucon.Nodes.Count, $"{Param.InputFileName} Nr of nodes");
            Assert.AreEqual(4, Glaucon.NodeRestraints.Count, $"{Param.InputFileName} Nr of restrained nodes");
            Assert.AreEqual(3, Glaucon.LoadCases.Count, $"{Param.InputFileName} Nr of load cases");

            for (var i = 0; i < Glaucon.LoadCases.Count; i++)
            {
                var lc1 = Glaucon.LoadCases[i];
                Assert.IsNotNull(lc1, $"Load case {i + 1} set to NULL");
                Assert.AreEqual(n[i, 0], lc1.NodalLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of loaded nodes");
                Assert.AreEqual(n[i, 1], lc1.UniformLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of uniform loads");
                Assert.AreEqual(n[i, 2], lc1.TrapLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of Trap loads");
                Assert.AreEqual(n[i, 3], lc1.IntPointLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of Conc. loads");
                Assert.AreEqual(n[i, 4], lc1.TempLoads.Count, $"{Param.InputFileName} Load case {i + 1} Nr of temperature loads");
                Assert.AreEqual(n[i, 5], lc1.PrescrDisplacements.Count, $"{Param.InputFileName} Load case {i + 1} Nr of pescr. displ");
            }


            //Ku.PermuteColumns(gl.Glaucon.Perm);
            //Ku.PermuteRows(gl.Glaucon.Perm);
            //CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 3, 1e-13, $"{Param.InputFileName} Ku");
            //Assert.AreEqual(Param.DynamicModesCount, 6, $"{file} Nr of dyn.modes nM");

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

            foreach(var mb in  Glaucon.Members)
            {
                var Q1 = mbr.Q;
                    CheckVector(Q1, sollQ[mb.Nr].Row(mb.Nr), 2,
                        $"{Param.InputFileName} EndForces for member {mb.Nr+1}");
              
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
            var sollv = (DenseVector) Vector<double>.Build.DenseOfArray(new[]
                {18.807943, 19.105451, 19.690439, 31.711570, 35.159165, 42.248953});
            //Assert.AreEqual( 3,gl.Glaucon.Param.FrequenciesFound,  "Eigenfrequensies found: ");

            CheckVector(gl.Glaucon.eigenFreq, sollv, 3, $"{Param.InputFileName} EigenFrequencies ");

            
            foreach( var mb in Glaucon.Members)
            {
                CheckVector(mb.maxPeakDisplacements, sollPeakDispl[mb.Nr], 3,
                    $"{Param.InputFileName} Maximum Peak Displacements {mb.Nr + 1}");
                CheckVector(mb.minPeakDisplacements, sollPeakDispl[mb.Nr], 3,
                    $"{Param.InputFileName} Minimum Peak Displacements member {mb.Nr + 1}");
            }

            foreach(var mb in Glaucon.Members)
            {
                CheckVector(mb.maxPeakForces, 4,$"{Param.InputFileName} Maximum Peak forces  member {mb.Nr+ 1}");
                CheckVector(mb.minPeakForces, sollq[mb.Nr], 4,$"{Param.InputFileName} Minimum Peak forces , member {mb.Nr + 1}");
            }

            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }
    }
}
