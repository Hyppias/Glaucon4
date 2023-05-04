using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using gl = Terwiel.Glaucon;
using MathNet.Numerics;
using Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestA : UnitTestBase
    {
        [TestMethod]
        public void TestA()
        {           
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            Assert.AreEqual(result, 0, $"Error computing {Param.InputFileName}");
            Assert.AreEqual(Glaucon.Nodes.Count, 12, $"{Param.InputFileName} # nodes");
            Assert.AreEqual(Glaucon.Members.Count, 21, $"{Param.InputFileName} # members");
            Assert.AreEqual(Glaucon.NodeRestraints.Count, 12, $"{Param.InputFileName} # nodes with Reactions");
            Assert.AreEqual(Glaucon.LoadCases.Count, 2, $"{Param.InputFileName} # load cases");
            Assert.AreEqual(Glaucon.AnimatedModes.Length, 0, $"{Param.InputFileName} # dynamic modes");

            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            //bool AlmostEqualRelative<T>(this Matrix<T> a, Matrix<T> b, double maximumError)

            Debug.Assert(Glaucon.LoadCases[0].Ku.AlmostEqualRelative(Ku, System.Math.Pow(10, -5)));

            // CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 5, $"{Param.InputFileName} Ku");
            
            foreach (var lc in Glaucon.LoadCases)
            {
                int i = lc.Nr;
                Assert.AreEqual(lc.TempLoads.Count, k[i, 0], $"{Param.InputFileName} # temperature loads load case {i + 1}");
                Assert.AreEqual(lc.NodalLoads.Count, k[i, 1], $"{Param.InputFileName} # loaded nodes load case {i + 1}");
                Assert.AreEqual(lc.PrescrDisplacements.Count, k[i, 2], $"{Param.InputFileName} # prescribed displacements load case {i + 1}");

                CheckVector(Glaucon.LoadCases[i].MechForces.Column(0), FMechSoll.Column(i), 7, $"{Param.InputFileName} FMech lc={i + 1}");

                //Debug.Assert(lc.Displacements.Column(0).AlmostEqualRelative(sollDispl.Row(i), System.Math.Pow(10,-8)),
                //    $"{Param.InputFileName} Displacements LoadCase {i + 1} "
                //    );

                CheckVector(lc.Displacements.Column(0), sollDispl.Row(i), 8,
                    $"{Param.InputFileName} Displacements LoadCase {i + 1} ");

                //Debug.Assert(lc.Reactions.AlmostEqualRelative(ReactionSoll[i].Transpose(), System.Math.Pow(10,-8)),
                //    $"{Param.InputFileName} Reactions lc={i+1}");

                CheckMatrix(lc.Reactions, ReactionSoll[i].Transpose(), 9, $"{Param.InputFileName} Reactions lc={i + 1}");

            }
        }
    }
}
