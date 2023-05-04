
using System.ComponentModel.DataAnnotations;
using System;
using Terwiel.Glaucon;
using Windows.UI.ViewManagement;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace UnitTestGlaucon
{
    public class TestGobject
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
            Analyze = true,
            Validate = true,
            ModalExaggeration = 7.0,
            DeformationExaggeration = 1.0,
            XIncrement = 12.0,
            UnifLoadsLocal = true,
            // HTML = 1, Latex = 2, CSV = 3, Excel = 4, XML = 5
            OutputFormat = 2,
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
                new( 1,new[]{-100,  0.0,   0}, 0),
                new( 2,new[]{ 100,  0.0,   0}, 0),
                new( 3,new[]{   0, 70.0,   0}, 0),
                new( 4,new[]{-100,  0.0,  80}, 0),
                new( 5,new[]{ 100,  0.0,  80}, 0),
                new( 6,new[]{   0, 70.0,  80}, 0),
                new( 7,new[]{-100,  0.0, 180}, 0),
                new( 8,new[]{ 100,  0.0, 180}, 0),
                new( 9,new[]{   0, 70.0, 180}, 0),
                new(10,new[]{-100,  0.0, 310}, 0),
                new(11,new[]{ 100,  0.0, 310}, 0),
                new(12,new[]{   0, 70.0, 310}, 0),
                new(13,new[]{-100,  0.0, 510}, 0),
                new(14,new[]{ 100,  0.0, 510}, 0),
                new(15,new[]{   0, 70.0, 510}, 0)
        },

            NodesRestraints = new List<NodeRestraint>
            {
                new(1,new[]{1,1,1,1,1,1}),
                new(2,new[]{1,1,1,1,1,1}),
                new(3,new[]{1,1,1,1,1,1})
        },
            Members = new List<Member>
            {
                new( 1, 1, 4,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 2, 2, 5,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 3, 3, 6,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 4, 4, 7,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 5, 5, 8,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 6, 6, 9,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 7, 7,10,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 8, 8,11,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new( 9, 9,12,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(10,10,13,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(11,11,14,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(12,12,15,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(13, 4, 5,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(14, 5, 6,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(15, 6, 4,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(16, 7, 8,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(17, 8, 9,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(18, 9, 7,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(19,10,11,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(20,11,12,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(21,12,10,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(22,13,14,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(23,14,15,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0),
                new(24,15,13,new[]{100,60,60.0},new[]{500,1000,1000.0},new[]{29000,11500,7.33e-7},0)
              },
            LoadCases = new List<LoadCase>
            {
                new LoadCase
                {
                    g=new double[] {0,0,-386.4},
                    NodalLoads = new List<NodalLoad>
                    {
                        new(  15,new[]{ 0d,200,0,0,0,0})
                    },
                    UniformLoads = new List<UniformLoad>
                    {
                        new(13,new[]{0.0,0,-2.361}),
                        new(14,new[]{0.0,0,-2.361}),
                        new(15,new[]{0.0,0,-2.361}),
                        new(16,new[]{0.0,0,-2.361}),
                        new(17,new[]{0.0,0,-2.361}),
                        new(18,new[]{0.0,0,-2.361}),
                        new(19,new[]{0.0,0,-2.361}),
                        new(20,new[]{0.0,0,-2.361}),
                        new(21,new[]{0.0,0,-2.361}),
                        new(22,new[]{0.0,0,-2.361}),
                        new(23,new[]{0.0,0,-2.361}),
                        new(24,new[]{0.0,0,-2.361})
                    }

                }
            },
            ExtraNodeInertias = new List<ExtraNodeInertia>
            {
                new(6,new[]{4.0,4.75e6,3.62e6,26.53e6}),
                new(19,new[]{4.0,4.75e6,3.62e6,26.53e6}),
                new(26,new[]{4.0,4.75e6,3.62e6,26.53e6}),
                new(33,new[]{4.0,4.75e6,3.62e6,26.53e6}),

        },
            AnimatedModes = new[] { 1, 2, 4, 4 }


        };
    };
}


