
using System.ComponentModel.DataAnnotations;
using System;
using Terwiel.Glaucon;
using Windows.UI.ViewManagement;
using static Terwiel.Glaucon.Glaucon;
using static Terwiel.Glaucon.Glaucon.LoadCase;
using System.Runtime.ConstrainedExecution;

namespace UnitTestGlaucon
{
    public partial class UnitTestJ
    {
        public Parameters Param = new Parameters()
        {
            ModalExaggeration = 0.8,//exaggerate modal mesh deformations  
            LumpedMassMatrix = false, // 0: consistent mass ... 1: lumped mass matrix   
            DynamicModesCount = 22,//number of desired dynamic modes of vibration
            XIncrement = -1,
            AccountForShear = true,
            AccountForGeomStability = false,
            CondensationMethod = 2,
            DeformationExaggeration = 1.0,
            ModalMethod = 1,//1: subspace Jacobi     2: Stodola 
            Scale = 1.0,
            PanRate = 3.0, // pan during animation 
            Shift = 10.0,//modal frequency shift factor  
            ModalConvergenceTol = 1e-6,//mode shape tolerance  
            StrainLimit = 12.0,
            EquilibriumTolerance = 12.0,
            EquilibriumError = 12.0,
            MinEigenvalue = 12.0,
            MaxEigenvalue = 12.0,
            ResidualTolerance = 1e-6,
            Analyze = true,
            Validate = true,
            UnifLoadsLocal = true,
            // HTML = 1, Latex = 2, CSV = 3, Excel = 4, XML = 5
            OutputFormat = 2,
            RenumNodes = true,
            MaxSegmentCount = 20,
            MinimumIterations = 2,
            MaximumIterations = 13,
            Decimals = 2,
            InputSource = 1
        };

        public Glaucon Glaucon = new()
        {

            Title = "Example J: tesseract  (N, mm, ton) ",

            Nodes = new List<Node>
            {
                new( 1,new[]{ 100, 100.0,-100},0),
new( 2,new[]{-100, 100.0,-100},0),
new( 3,new[]{-100,-100.0,-100},0),
new( 4,new[]{ 100,-100.0,-100},0),
new( 5,new[]{ 100, 100.0, 100},0),
new( 6,new[]{-100, 100.0, 100},0),
new( 7,new[]{-100,-100.0, 100},0),
new( 8,new[]{ 100,-100.0, 100},0),
new( 9,new[]{  70,  70.0, -70},0),
new(10,new[]{ -70,  70.0, -70},0),
new(11,new[]{ -70, -70.0, -70},0),
new(12,new[]{  70, -70.0, -70},0),
new(13,new[]{  70,  70.0,  70},0),
new(14,new[]{ -70,  70.0,  70},0),
new(15,new[]{ -70, -70.0,  70},0),
new(16,new[]{  70, -70.0,  70},0)


            },

            Members = new List<Member>
            {
                new( 1, 1, 2,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 2, 2, 3,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 3, 3, 4,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 4, 4, 1,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 5, 5, 6,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 6, 6, 7,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 7, 7, 8,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 8, 8, 5,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new( 9, 9,10,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(10,10,11,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(11,11,12,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(12,12, 9,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(13,13,14,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(14,14,15,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(15,15,16,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(16,16,13,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(17, 1, 5,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(18, 2, 6,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(19, 3, 7,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(20, 4, 8,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(21, 9,13,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(22,10,14,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(23,11,15,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(24,12,16,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(25, 1, 9,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(26, 2,10,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(27, 3,11,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(28, 4,12,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(29, 5,13,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(30, 6,14,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(31, 7,15,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0),
                new(32, 8,16,new[]{100,60.0,60},new[]{500.0,1000,1000},new[]{9990.0,3800,2.73e-9},0)

                },
            LoadCases = new List<LoadCase>
            {
                new LoadCase
                {
                    g=new double[] {0,0,0 },

                }
            },

            AnimatedModes = new[] { 7, 10, 12, 13, 15, 18, 20, 22 },

        };
    };
}


