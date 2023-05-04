
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace Terwiel.Glaucon
{
    public class InputData
    {
        Parameters Param = new Parameters
        {
            StrainLimit = 12.0,
            EquilibriumTolerance = 12.0,
            EquilibriumError = 12.0,
            MinEigenvalue = 12.0,
            MaxEigenvalue = 12.0,
            ModalConvergenceTol = 12.0,
            ResidualTolerance = 12.0,            
            Analyze = true,            
            Validate = true,
            UnifLoadsLocal = true,
            // HTML = 1, Latex = 2, CSV = 3, Excel = 4, XML = 5
            OutputFormat = 2,
            RenumNodes = true,
            MaxSegmentCount = 20,
            // AxialStrainWarning = 0, // is OUTPUT!
            MinimumIterations = 2,
            MaximumIterations = 13,
            // FrequenciesFound = 1, is output!
            // LoopCount = 12, // is out put
            //Iterations = 12, // is output
            Decimals = 2,
            ModalMethod = 0, // 0 = none
            // 0 = Glaucon object, 1=JSON,2 = FRAME3DD
            InputSource = 1,
            InputPath = $"{ProjectDir}Glaucon4\\Resources\\",
            OutputPath = $"{ProjectDir}Glaucon4\\Resources\\Results\\",
            InputFileName = "DeaultInput.json"
        };

        const string ProjectDir = "E=\\Users\\erik\\Documents\\Visual Studio 2022\\Projects\\Glaucon4\\";
        private Glaucon glaucon = new()
        {

            Title = "Example A: linear static analysis of a 2D truss with support settlement (kips,in) ",

            Nodes = new List<Glaucon.Node>
            {

            new (1, new[] { 0.0, 0.0, 0.0 }, 0.0),
            new (2, new[] { 120.0, 0.0, 0.0 }, 0.0),
            new(3,  new[] { 240.0, 0.0, 0.0 }, 0.0),
            new(4,  new[] { 360.0, 0.0, 0.0 }, 0.0),
            new(5,  new[] { 480.0, 0.0, 0.0 }, 0.0),
            new(6,  new[] { 600.0, 0.0, 0.0 }, 0.0),
            new(7,  new[] { 720.0, 0.0, 0.0 }, 0.0),
            new(8,  new[] { 120.0, 120.0, 0.0 }, 0.0),
            new(9,  new[] { 240.0, 120.0, 0.0 }, 0.0),
            new(10, new[] { 360.0, 120.0, 0.0 }, 0.0),
            new(11, new[] { 480.0, 120.0, 0.0 }, 0.0),
            new(12, new[] { 600.0, 120.0, 0.0 }, 0.0)
            },

            NodesRestraints = new List<NodeRestraint>
            {
                new(1,  new [] {1,1,1,1,1,0}),
                new(2,  new [] {0,0,1,1,1,0}),
                new(3,  new [] {0,0,1,1,1,0}),
                new(4,  new [] {0,0,1,1,1,0}),
                new(5,  new [] {0,0,1,1,1,0}),
                new(6,  new [] {0,0,1,1,1,0}),
                new(7,  new [] {0,0,1,1,1,0}),
                new(8,  new [] {0,0,1,1,1,0}),
                new(9,  new [] {0,0,1,1,1,0}),
                new(10,  new [] {0,0,1,1,1,0}),
                new(11,  new [] {0,0,1,1,1,0}),
                new(12,  new [] {0,0,1,1,1,0})
            },

            Members = new List<Member>
            {
                new( 1,  1,  2 ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 2,  2,  3  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 3,  3,  4  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 4,  4,  5  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 5,  5,  6  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 6,  6,  7  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 7,  1,  8  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 8,  2,  8  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new( 9,  2,  9  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(10,  3,  9  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(11,  4,  9  ,  new[] {10.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(12,  4,  10,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(13,  4,  11,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(14,  5,  11,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(15,  6,  11,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(16,  6,  12,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(17,  7,  12,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(18,  8,   9,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(19,  9,  10,     new[] {10.0    ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(20, 10,  11,   new[] { 0.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
                new(21, 11,  12,   new[] { 0.0  ,1.0    ,1.0},  new[] {1.0  ,1.0    ,0.01}, new[] {29000, 11500,  0 , 7.33e-7 },0),
            },

            LoadCases = new List<LoadCase>
            {

                new LoadCase(
                    1, // load case number
                    new double []{ 0, 0, 0 },// must be double
                    new List<NodalLoad>
                    {
                        //new(2, new[] { 0.0, -10.0, 0.0, 0.0, 0.0, 0.0 }),
                        //new(3, new[] { 0.0, -20.0, 0.0, 0.0, 0.0, 0.0 }),
                        //new(4, new[] { 0.0, -20.0, 0.0, 0.0, 0.0, 0.0 }),
                        //new(5, new[] { 0.0, -10.0, 0.0, 0.0, 0.0, 0.0 }),
                        //new(6, new[] { 0.0, -20.0, 0.0, 0.0, 0.0, 0.0 })
                    },
                    new List<UniformLoad>
                    {
                        new(1,new double[]{1,1,1}),
                        new(2,new double[]{1,1,1}),
                    },

                    new List<TrapLoad>
                    {
                        //new (  1, new [] {  new TrapLoad.Load((1,1,1,1)) ,
                        //                    new TrapLoad.Load((1,1,1,1)),
                        //                    new TrapLoad.Load((1,1,1,1))
                        //                 }
                        //    )
                    },
                    new List<TempLoad>
                    {
                        new(10, new []{6e-12, 5.0, 5.0, 10, 10, 10, 10 }),
                        new(13, new []{6e-12, 5.0, 5.0, 15, 15, 15, 15 }),
                        new(15, new []{6e-12, 5.0, 5.0, 17, 17, 17, 17 })
                    },
                    new List<IntPointLoad>
                    {
                       // new (2,10,new[] {1.0,1,1 })
                    },
                    new List<PrescrDisplacement>
                    {
                        new(8, new[] { 0.1, 0.0, 0.0, 0.0, 0.0, 0.0 })
                    }

                ), // end LoadCase
                 new LoadCase( // second load case
                    2, // load case number
                    new double []{ 0, 0, 0 },// must be double
                    new List<NodalLoad>
                    {

                        new(3, new[] { 20.0, -20.0, 0.0, 0.0, 0.0, 0.0 }),
                        new(4, new[] { 10.0, -20.0, 0.0, 0.0, 0.0, 0.0 }),
                        new(5, new[] { 20.0, -10.0, 0.0, 0.0, 0.0, 0.0 }),

                    },
                    new List<UniformLoad>
                    {

                    },

                    new List<TrapLoad>
                    {

                    },
                    new List<TempLoad>
                    {

                    },
                    new List<IntPointLoad>
                    {
                       // new (2,10,new[] {1.0,1,1 })
                    },
                    new List<PrescrDisplacement>
                    {
                        new(1, new[] { 0.0, -1.0, 0.0, 0.0, 0.0, 0.0 }),
                        new(8, new[] { 0.1,  0.0, 0.0, 0.0, 0.0, 0.0 })
                    }

                ), // end LoadCase
            }, // End LoadCases
            ExtraNodeInertias = new List<ExtraNodeInertia>
            {
                //new(1, new double[] { 1,1,1,1 })
            },
            ExtraElementMasses = new List<ExtraElementMass>
            {
                //new ExtraElementMass(1,3)
            },
            AnimationModes = null, //new int[] { 1, 2, 3 },
            CondensedNodes = new List<CondensedNode>
            {
                //new(1,new int[]{0,0,0,1,1,1})
            }

        };
    }
}
