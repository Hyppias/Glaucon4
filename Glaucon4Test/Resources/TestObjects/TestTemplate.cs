
using System.ComponentModel.DataAnnotations;
using System;
using Terwiel.Glaucon;
using Windows.UI.ViewManagement;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace UnitTestGlaucon
{
    public class Template
    {
        public Parameters Param = new Parameters()
        {

            StrainLimit = 12.0,
            EquilibriumTolerance = 12.0,
            EquilibriumError = 12.0,
            MinEigenvalue = 12.0,
            MaxEigenvalue = 12.0,
            ModalConvergenceTol = 1e-6,
            ResidualTolerance = 1e-6,
            //MaxVibrationTime = 12.0,
            Analyze = true,
            Validate = true,
            ModalExaggeration = 7.0,
            DeformationExaggeration = 1.0,
            XIncrement = 12.0,
            UnifLoadsLocal = true,
            
            OutputFormat = 2,// HTML = 1, Latex = 2, CSV = 3, Excel = 4, XML = 5
            RenumNodes = true,
            MaxSegmentCount = 20,
            MinimumIterations = 2,
            MaximumIterations = 13,
            AccountForShear = true,
            AccountForGeomStability = true,
            CondensationMethod = 2,
            Decimals = 2,
            ModalMethod = 1,
            Scale = 1.2,
            LumpedMassMatrix = false,
            DynamicModesCount = 4,
            PanRate = 3.0,
            Shift = 0.0,
            InputSource = 1
        };

        public Glaucon Glaucon = new()
        {

            Title = "Example G: a building with a set-back  (in,kip)",

            Nodes = new List<Node>
            {
                // new( 1,new[]{-100,  0.0,   0}, 0),
            },

            NodesRestraints = new List<NodeRestraint>
            {
                // new(1,new[]{1,1,1,1,1,1}),
            },
            Members = new List<Member>
            {
                // new( 1, 1, 4,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
            },
            LoadCases = new List<LoadCase>
            {
                new LoadCase
                {
                    g=new double[] {0,0,-386.4},
                    NodalLoads = new List<NodalLoad>
                    {
                        //new(  15,new[]{ 0d,200,0,0,0,0})
                    },
                    UniformLoads = new List<UniformLoad>
                    {
                        //new(13,new[]{0.0,0,-2.361}),                        
                    },
                    TrapLoads = new List<TrapLoad>
                    {
                        new(9 ,new TrapLoad.Load[]
                            {                                 
                                //new ((0,   0,  0  ,  0  )),                                     
                            })
                    },
                    TempLoads = new List<TempLoad>
                    {
                        // new(1, new []{   12e-6,    10,   10,  20,   10,   10,  -10 })
                    },
                    IntPointLoads = new List<IntPointLoad>
                    {
                        // new (2, new [] { 23,3455,45.0 },644,  true)
                         
                    },
                }
            },
            ExtraNodeInertias = new List<ExtraNodeInertia>
            {
                // new(6,new[]{4.0,4.75e6,3.62e6,26.53e6}),
            },
            ExtraElementMasses = new List<ExtraElementMass>
            {
                //new(45   ,26e-3)
            },
            AnimatedModes = new[] { 1, 2, 4, 4 },
            NodesToCondense = new List<CondensedNode>
            {
                //  new(  6,new[]{1,1,0,0,0,1}),
            },
            MatchedCondenseModes = new[] { 1, 2, 3, 4, 5 }

        };
    };
}


