
using System.ComponentModel.DataAnnotations;
using System;
using Terwiel.Glaucon;
using Windows.UI.ViewManagement;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;

namespace UnitTestGlaucon
{
    public class TestEobject
    {
        Parameters Param = new Parameters()
        {

            StrainLimit = 12.0,
            EquilibriumTolerance = 12.0,
            EquilibriumError = 12.0,
            MinEigenvalue = 12.0,
            MaxEigenvalue = 12.0,
            ModalConvergenceTol = 1e-5,
            ResidualTolerance = 1e-6,
            MaxVibrationTime = 12.0,
            Analyze = true,
            DoModal = true,
            Validate = true,
            ModalExaggeration = 10.0,
            XIncrement = 6.0,
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
            DeformationExaggeration = 2.0,
            Decimals = 2,
            ModalMethod = 1,
            Scale = 1.5,
            ConsistentMassMatrix = true,
            LumpedMassMatrix = false,
            DynamicModesCount = 14,
            PanRate = 2.0,
            Shift = 1.0,
            InputSource = 1
        };

        private Glaucon glaucon = new()
        {

            Title = "Example E: a three dimensional structure showing lateral-torsional dynamic modes (units: kip, in)",

            Nodes = new List<Node>
            {
                new(   1, new[]{    0.0,  0,   0}, 0),  
                new(   2, new[]{   72.0,  0,   0}, 0),  
                new(   3, new[]{  144.0,  0,   0}, 0),  
                new(   4, new[]{  144.0, 36,   0}, 0),  
                new(   5, new[]{  144.0, 72,   0}, 0),  
                new(   6, new[]{   72.0, 72,   0}, 0),  
                new(   7, new[]{    0.0, 72,   0}, 0),  
                new(   8, new[]{    0.0, 36,   0}, 0),  
                new(   9, new[]{    0.0,  0,-120}, 0),  
                new(  10, new[]{  144.0,  0,-120}, 0),  
                new(  11, new[]{   72.0, 72,-120}, 0),  
                new(  12, new[]{   72.0, 36,   0}, 0),  

            },

            NodesRestraints = new List<NodeRestraint>
            {
                new(  9,new[]{1,1,1,1,1,1}),   
                new( 10,new[]{1,1,1,1,1,1}),   
                new( 11,new[]{1,1,1,1,1,1})   

            },
            Members = new List<Member>
            {
                new(  1,  1, 2,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  2,  2, 3,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  3,  3, 4,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  4,  4, 5,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  5,  5, 6,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  6,  6, 7,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  7,  7, 8,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  8,  8, 1,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new(  9,  9, 1,new[]{ 1100.0, 800, 800},new[]{  001.0, 110,110},new[]{  29000, 11500, 7e-7},0),  
                new( 10, 10, 3,new[]{ 1100.0, 800, 800},new[]{  001.0, 110,110},new[]{  29000, 11500, 7e-7},0),  
                new( 11, 11, 6,new[]{ 1100.0, 800, 800},new[]{  001.0, 110,110},new[]{  29000, 11500, 7e-7},0),  
                new( 12, 12, 2,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new( 13, 12, 4,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new( 14, 12, 6,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  
                new( 15, 12, 8,new[]{ 1100.0, 800, 800},new[]{ 1000.0, 500,500},new[]{ 999000, 11500, 7e-7},0),  


                },
            LoadCases = new List<LoadCase>
            {
                new LoadCase
                {
                    g=new double[] {0,0,0 },
                    NodalLoads = new List<NodalLoad>
                    {
                        new(3,new[]{0,500.0,-500,0,0,0})
                    },
                     TrapLoads = new List<TrapLoad>
                        {
                           new(9 ,new TrapLoad.Load[]        
                                {                                 
                                    new ((0,   0,  0  ,  0  )),  
                                    new ((0,   0,  0  ,  0  )),  
                                    new ((0, 120, 0.00, 0.20)),  
                                }),                               
                            new(10 ,new TrapLoad.Load[]      
                                {                            
                                new ((0,   0,  0  ,  0  )), 
                                new ((0,   0,  0  ,  0  )), 
                                new ((0, 120, 0.00, 0.30)), 
                            }),                              
                            new(11 ,new TrapLoad.Load[]       
                                    {                        
                                new ((0,   0,  0  ,  0  )), 
                                new ((0,   0,  0  ,  0  )), 
                                new ((0, 120, 0.00, 0.40)), 
                                })                                
                        },
                    
                  
                } 
            },
            ExtraNodeInertias = new List<ExtraNodeInertia>
            {
                new (12, new[]{ 3.388,0,0,939,37})
            },
            AnimatedNodes = new[] { 1, 2,3,4 },
            NodesToCondense = new List<CondensedNode>
            {
                new (12, new[]{1,1,0,0,0,1})
            }
            

        };
    };
}


