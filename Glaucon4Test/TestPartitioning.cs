using MathNet.Numerics;
using NUnit.Framework.Interfaces;
using System.Reflection;
using Terwiel.Glaucon;
using static Terwiel.Glaucon.Glaucon;

namespace UnitTestGlaucon
{
    [TestFixture]
    public partial class TestPartitioning : UnitTestBase
    { 
        [Test]
        public void TestPart()
        {   
            // Arrange

            var glaucon = new Glaucon()
            { 
                NodesRestraints= new List<NodeRestraint>
                {
                    new(  0, new[] {1, 1, 1, 1, 1, 0}),
                    new(  1, new[] {0, 0, 1, 1, 1, 0}),
                    new(  2, new[] {0, 0, 1, 1, 1, 0}),
                    
                },
                Nodes = new List<Node>
                {
                    new( 0, new[]{0.0,0,0 }, 0.0), // don't need coords
                    new( 1, new[]{0.0,0,0 }, 0.0),
                    new( 2, new[]{0.0,0,0 }, 0.0)
                },
                
            };
            Glaucon.Param =  new Parameters() { Analyze = false , Validate = false};
            DoF = 6 * glaucon.Nodes.Count;

            // Analyse
            var result = glaucon.PartitionSystemMatrices(); 
            
            // Act
            for(int i=0; i < DoF; i++)
            {
                Assert.That(result[i] , Is.EqualTo(new [] {5,6,7,   11,12,13,17, 0,1,2,3,4, 8,9,10, 14,15,16}[i]), $"Index {i}, being {result[i]}, is out of range.");
            }
            Assert.That(Glaucon.FreeCount,Is.EqualTo(7),"There should be 7 free DoFs.");

        }
    }
}
