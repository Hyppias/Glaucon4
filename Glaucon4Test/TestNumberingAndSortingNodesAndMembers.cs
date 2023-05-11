using MathNet.Numerics;
using NUnit.Framework.Interfaces;
using System.Reflection;
using Terwiel.Glaucon;
using static Terwiel.Glaucon.Glaucon;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class TestRenumberNodesAndMembers : UnitTestBase
    {
        [Test]
        public void TestRenumbering()
        {
            // Arrange

            var glaucon = new Glaucon()
            {
                Nodes = new List<Node>
                {
                    new( 1, new[]{  0.0   , 0.0    ,0.0    }, 0.0),
                    new( 2, new[]{  120.0 , 0.0    ,0.0    }, 0.0),
                    new( 6, new[]{  600.0 , 0.0    ,0.0    }, 0.0),
                    new( 3, new[]{  240.0 , 0.0    ,0.0    }, 0.0),
                    new( 4, new[]{  360.0 , 0.0    ,0.0    }, 0.0),
                    new( 5, new[]{  480.0 , 0.0    ,0.0    }, 0.0),
                },

                NodesRestraints = new List<NodeRestraint>
                {
                    new(  1, new[] {1, 1, 1, 1, 1, 0}),// 0 = free, 1=fixed
                    
                    new(  4, new[] {0, 0, 1, 1, 1, 0}),
                    new(  5, new[] {0, 0, 1, 1, 1, 0}),
                    new(  2, new[] {0, 0, 1, 1, 1, 0}),
                    new(  3, new[] {0, 0, 1, 1, 1, 0}),
                    new(  6, new[] {0, 0, 1, 1, 1, 0}),
                },

                Members = new List<Member>
            {//      n  A  B          Area                  Ix   Iy    Ip            E     G     rho     alpha   r   
                new( 1, 1,  2,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),

                new( 4, 4,  5,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 5, 5,  6,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 2, 2,  3,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 3, 3,  4,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                }

            };
            Glaucon.Param = new Parameters() { Analyze = false, Validate = false };
           
            // Analyse
            glaucon.ArrangeNodesAndNumbers();

            // Act
            for (int i = 0; i < glaucon.Nodes.Count; i++)
            {
                Assert.That(glaucon.Nodes[i].Nr, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }[i]),
                    $"Node nr {i} not equal to {glaucon.Nodes[i].Nr}.");
            }
            for (int i = 0; i < glaucon.Members.Count; i++)
            {
                Assert.That(glaucon.Members[i].Nr, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }[i]),
                    $"Member nr {i} not equal to {glaucon.Members[i].Nr}.");
                Assert.That(glaucon.Members[i].NodeA.Nr, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }[i]),
                   $"Member {i} NodeA not equal to {glaucon.Members[i].NodeB.Nr}.");
                Assert.That(glaucon.Members[i].NodeB.Nr, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }[i]),
                   $"Member {i} NodeB not equal to {glaucon.Members[i].NodeB.Nr}.");
            }
            for (int i = 0; i < glaucon.NodesRestraints.Count; i++)
            {
                Assert.That(glaucon.NodesRestraints[i].NodeNr, Is.EqualTo(new[] { 0, 1, 2, 3, 4,5 }[i]),
                    $"Member nr {i} not equal to {glaucon.NodesRestraints[i].NodeNr}.");
            }
        }
    }
}
