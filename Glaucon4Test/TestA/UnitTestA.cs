using MathNet.Numerics;
using System.Reflection;
using Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class UnitTestA : UnitTestBase
    {       
        
        [Test]
        public void TestA()
        {            
            Glaucon.ProcessGlaucon(Param);
            Glaucon.BaseFile = MethodBase.GetCurrentMethod().Name;
            var result = Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            Assert.That(result == 0, $"Error computing {Param.InputFileName}");
            Assert.That(Glaucon.Nodes.Count == 12, $"{Param.InputFileName} # nodes");
            Assert.That(Glaucon.Members.Count == 21, $"{Param.InputFileName} # members");
            Assert.That(Glaucon.NodesRestraints.Count == 12, $"{Param.InputFileName} # nodes with Reactions");
            Assert.That(Glaucon.LoadCases.Count == 2, $"{Param.InputFileName} # load cases");
            Assert.That(Glaucon.AnimatedModes.Length == 0, $"{Param.InputFileName} # dynamic modes");

#if DEBUG
            Ku.PermuteColumns(gl.Glaucon.Perm);
            Ku.PermuteRows(gl.Glaucon.Perm);
            //bool AlmostEqualRelative<T>(this Matrix<T> a, Matrix<T> b, double maximumError)

            Debug.Assert(Glaucon.LoadCases[0].Ku.AlmostEqualRelative(Ku, System.Math.Pow(10, -5)));

            // CheckMatrix(Glaucon.LoadCases[0].Ku, Ku, 5, $"{Param.InputFileName} Ku");
#endif
            foreach (var lc in Glaucon.LoadCases)
            {
                int i = lc.Nr;
                Assert.That(lc.TempLoads.Count == k[i, 0], $"{Param.InputFileName} # temperature loads load case {i + 1}");
                Assert.That(lc.NodalLoads.Count == k[i, 1], $"{Param.InputFileName} # loaded nodes load case {i + 1}");
                Assert.That(lc.PrescrDisplacements.Count == k[i, 2], $"{Param.InputFileName} # prescribed displacements load case {i + 1}");

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
