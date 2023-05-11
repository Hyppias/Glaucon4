
using Terwiel.Glaucon;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace UnitTestGlaucon
{

    public partial class UnitTestA :UnitTestBase
    {

        public Parameters Param = new Parameters()
        {
            StrainLimit = 12.0,
            EquilibriumTolerance = 12.0,
            EquilibriumError = 12.0,
            //MinEigenvalue = 12.0,
            //MaxEigenvalue = 12.0,
            //ModalConvergenceTol = 1e-6,
            ResidualTolerance = 1e-6,
            Analyze = true,
            Validate = true,
            //ModalExaggeration = 20.0,
            XIncrement = -1,
            UnifLoadsLocal = true,
            OutputFormat = 2, // HTML = 1, Latex = 2, CSV = 3, Excel = 4, XML = 5
            RenumNodes = false,
            MaxSegmentCount = 20,
            MinimumIterations = 2,
            MaximumIterations = 13,
            AccountForShear = false,
            AccountForGeomStability = false,
            DeformationExaggeration = 50.0,
            Decimals = 2,
            // ModalMethod = 0, // 5 = MKL, 2 = STODOLA, 1 = SUBSPACE, 6 = FEAST, 3 = RANGE, 0 = None
            Scale = 1.0,
            //LumpedMassMatrix = true,
            DynamicModesCount = 0,
            PanRate = 2.0,
            Shift = 10.0,
            InputSource = 1, // object
            OutputPath = @"E:\Users\erik\Documents\Visual Studio 2022\Projects\Glaucon4\Glaucon4Test\Out\",
            InputPath = @"E:\Users\erik\Documents\Visual Studio 2022\Projects\Glaucon4\Glaucon4Test\Resources\"
        };
        public Glaucon Glaucon = new()

        {
            Title = "Example A: linear static analysis of a 2D truss with support settlement (kips,in) ",

            Nodes = new List<Node>
                {
                    new( 1, new[]{  0.0   , 0.0    ,0.0    }, 0.0),
                    new( 2, new[]{  120.0 , 0.0    ,0.0    }, 0.0),
                    new( 3, new[]{  240.0 , 0.0    ,0.0    }, 0.0),
                    new( 4, new[]{  360.0 , 0.0    ,0.0    }, 0.0),
                    new( 5, new[]{  480.0 , 0.0    ,0.0    }, 0.0),
                    new( 6, new[]{  600.0 , 0.0    ,0.0    }, 0.0),
                    new( 7, new[]{  720.0 , 0.0    ,0.0    }, 0.0),
                    new( 8, new[]{  120.0 , 120.0, 0.0}    ,0.0 ),
                    new( 9, new[]{  240.0   , 120.0, 0.0}    ,0.0 ),
                    new(10, new[]{  360.0   , 120.0, 0.0}    ,0.0 ),
                    new(11, new[]{  480.0   , 120.0, 0.0}    ,0.0 ),
                    new(12, new[]{  600.0   , 120.0, 0.0}    ,0.0 )
                },

            NodesRestraints = new List<NodeRestraint>
                {
                    new(  1, new[] {1, 1, 1, 1, 1, 0}),// 0 = free, 1=fixed
                    new(  2, new[] {0, 0, 1, 1, 1, 0}),
                    new(  3, new[] {0, 0, 1, 1, 1, 0}),
                    new(  4, new[] {0, 0, 1, 1, 1, 0}),
                    new(  5, new[] {0, 0, 1, 1, 1, 0}),
                    new(  6, new[] {0, 0, 1, 1, 1, 0}),
                    new(  7, new[] {0, 1, 1, 1, 1, 0}),
                    new(  8, new[] {1, 0, 1, 1, 1, 0}),
                    new(  9, new[] {0, 0, 1, 1, 1, 0}),
                    new( 10, new[] {0, 0, 1, 1, 1, 0}),
                    new( 11, new[] {0, 0, 1, 1, 1, 0}),
                    new( 12, new[] {0, 0, 1, 1, 1, 0})
                },

            Members = new List<Member>
            {//      n  A  B          Area                  Ix   Iy    Ip            E     G     rho     alpha   r   
                new( 1, 1,  2,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 2, 2,  3,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 3, 3,  4,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 4, 4,  5,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 5, 5,  6,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 6, 6,  7,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 7, 1,  8,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 8, 2,  8,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new( 9, 2,  9,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(10, 3,  9,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(11, 4,  9,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(12, 4, 10,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(13, 4, 11,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(14, 5, 11,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(15, 6, 11,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(16, 6, 12,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(17, 7, 12,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(18, 8,  9,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(19, 9, 10,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(20,10, 11,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
                new(21,11, 12,new[]{ 10.0 ,1.0, 1.0}, new[]{1.0, 1.0, 0.01},new[]{29000, 11500, 7.33e-7, 6e-12 },0),
            },

            LoadCases = new List<LoadCase>
            {
                new LoadCase()
                {
                    // second load case
                    Nr = 1, // load case number
                    g = new double []{ 0, 0, 0 },// must be double
                   
                    NodalLoads = new List<NodalLoad>
                    {
                        new( 2, new double[]{0.0, -10.0, 0.0 ,0.0, 0.0, 0.0}),
                        new( 3, new double[]{0.0, -20.0, 0.0 ,0.0, 0.0, 0.0}),
                        new( 4, new double[]{0.0, -20.0, 0.0 ,0.0, 0.0, 0.0}),
                        new( 5, new double[]{0.0, -10.0, 0.0 ,0.0, 0.0, 0.0}),
                        new( 6, new double[]{0.0, -20.0, 0.0 ,0.0, 0.0, 0.0})
                    },
                    UniformLoads = new List<UniformLoad>
                    {
                        new (2, new double[]{ 0, 0.1,0}),
                        new (1, new double[]{ 0, 0,0.1})
                    },
                    PrescrDisplacements =  new List<PrescrDisplacement>
                    {
                        new(8, new[] { 0.1, 0.0, 0.0, 0.0, 0.0, 0.0 })
                    }
                }, // end LoadCase

                new LoadCase() // second load case
                    {
                        Nr = 2, // load case number
                        g = new[] { 0d, 0, 0 },// must be double
                        NodalLoads = new List<NodalLoad>
                        {
                            new( 3, new[]{  20.0, 0.0, 0.0 ,0.0,0.0 ,0.0 }),
                            new( 4, new[]{  10.0, 0.0, 0.0 ,0.0,0.0 ,0.0 }),
                            new( 5, new[]{  20.0, 0.0, 0.0 ,0.0,0.0 ,0.0 })
                        },
                        TempLoads = new List<TempLoad>
                        {
                            new(10, new[]{6e-12, 5.0 , 5.0, 10, 10, 10, 10}),
                            new(13, new[]{6e-12, 5.0 , 5.0, 15, 15, 15, 15}),
                            new(15, new[]{6e-12, 5.0 , 5.0, 17, 17, 17, 17})
                        },
                        PrescrDisplacements =  new List<PrescrDisplacement>
                            {
                                new(  1,new[]{  0.0, -1.0 ,0.0 ,0.0, 0.0, 0.0}),
                                new(  8,new[]{  0.1,  0.0 ,0.0 ,0.0, 0.0, 0.0})
                            }
                    },// end loadcase 3
                } // End LoadCases
        };
    }
}


