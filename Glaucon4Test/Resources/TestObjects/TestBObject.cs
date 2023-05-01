
using Terwiel.Glaucon;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace UnitTestGlaucon
{
    public class TestBobject
    {
        private Glaucon glaucon = new()
        {

            Title = "Example B: a pyramid-shaped frame --- static and dynamic analysis (N,mm,ton)",

            Nodes = new List<Node>
            {

            new (1, new[] { 0.0, 0.0, 1000.0 }, 0.0),
            new (2, new[] { -1200.0, -900.0, 0.0 }, 0.0),
            new(3,  new[] { 1200.0, -900.0, 0.0 }, 0.0),
            new(4,  new[] { 1200.0, 900.0, 0.0 }, 0.0),
            new(5,  new[] { -1280.0, 900.0, 0.0 }, 0.0)

            },

            NodesRestraints = new List<NodeRestraint>
            {
                new(2,  new [] {1,1,1,1,1,1}),
                new(3,  new [] {1,1,1,1,1,1}),
                new(4,  new [] {1,1,1,1,1,1}),
                new(5,  new [] {1,1,1,1,1,1})
            },

            Members = new List<Member>
            {
                new( 1,  2,  1 ,  new[] {36.0  ,20.0    ,20.0},  new[] {1000.0  ,492    ,492}, new[] {200000, 79300,  0 , 7.85e-9 },0),
                new( 2,  1,  3 ,  new[] {36.0  ,20.0    ,20.0},  new[] {1000.0  ,492    ,492}, new[] {200000, 79300,  0 , 7.85e-9 },0),
                new( 3,  1,  4 ,  new[] {36.0  ,20.0    ,20.0},  new[] {1000.0  ,492    ,492}, new[] {200000, 79300,  0 , 7.85e-9 },0),
                new( 4,  5,  1 ,  new[] {36.0  ,20.0    ,20.0},  new[] {1000.0  ,492    ,492}, new[] {200000, 79300,  0 , 7.85e-97},0),
            },

            LoadCases = new List<LoadCase>
                {
                new LoadCase()
                {
                    Nr = 1, // load case number
                    g = new double []{ 0, -9806.33, 0 },// must be double
                    NodalLoads = new List<NodalLoad>
                    {
                        new(1, new[] { 100, -200, -100, 0.0, 0.0, 0.0 })
                    },
                    UniformLoads = new List<UniformLoad>
                    {
                        new(1,new double[]{1,1,1}),
                        new(2,new double[]{1,1,1})
                    }
                }, // end LoadCase
                new LoadCase()
                {
                    // second load case
                    Nr = 2, // load case number
                    g = new double []{ 0, -9806.33, 0 },// must be double
                   
                    UniformLoads = new List<UniformLoad>
                    {
                        new (2, new double[]{ 0, 0.1,0}),
                        new (1, new double[]{ 0, 0,0.1})
                    },

                   

                }, // end LoadCase

        }, // End LoadCases
            ExtraNodeInertias = new List<ExtraNodeInertia>
            {
                new(1, new double[] { 0.1,0,0,0 })
            },

            AnimationModes = new int[] { 1, 2, 3, 4, 5, 6 }

        };
    };
}

