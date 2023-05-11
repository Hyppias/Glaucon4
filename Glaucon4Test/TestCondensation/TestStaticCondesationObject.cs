
using Terwiel.Glaucon;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace UnitTestGlaucon
{

    public partial class TestStaticCond
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
            Title = "Static Condensation",

            Nodes = new List<Node>
                {
                    new( 1, new[]{  0.0 , 0.0    ,0.0    }, 0.0),
                    new( 2, new[]{  0.0 , 4.0    ,0.0    }, 0.0),
                    new( 3, new[]{  8.0 , 4.0    ,0.0    }, 0.0),
                    new( 4, new[]{  8.0 , 0.0    ,0.0    }, 0.0),
                },

            NodesRestraints = new List<NodeRestraint>
                {
                    new(  1, new[] {1, 1, 1, 1, 1, 1}),// 0 = free, 1=fixed                 
                    new(  4, new[] {1, 1, 1, 1, 1, 1}),
                },

            Members = new List<Member>
            {//      n  A  B          Area                  Ix   Iy    Ip            E     G     rho     alpha   r   
                new( 1, 1,  2,new[]{ 1.0 ,1.0, 1.0}, new[]{1.0, 1.0, 1},new[]{1, 11500, 7.33e-7, 6e-12 },0),
                new( 2, 2,  3,new[]{ 1.0 ,1.0, 1.0}, new[]{1.0, 1.0, 1},new[]{1, 11500, 7.33e-7, 6e-12 },0),
                new( 3, 3,  4,new[]{ 1.0 ,1.0, 1.0}, new[]{1.0, 1.0, 1},new[]{1, 11500, 7.33e-7, 6e-12 },0),

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
                        new( 3, new double[]{10.0, 0.0, 0.0 ,0.0, 0.0, 0.0}),

                    },
                    UniformLoads = new List<UniformLoad>
                    {
                        new (2, new double[]{ 0, 10,0}),
                } // end LoadCase
                
            }, // End LoadCases
                
        }
            ,
            CondensedNodes = new List<CondensedNode>
            {
                new(1,new int[]{1,1,1,1,1,1}),
                new(4,new int[]{1,1,1,1,1,1}),
                new(2,new int[]{0,1,1,1,1,0}),
                new(3,new int[]{0,1,1,1,1,0})
            }
        };
    }
}




