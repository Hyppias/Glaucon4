
using System.ComponentModel.DataAnnotations;
using System;
using Terwiel.Glaucon;
using Windows.UI.ViewManagement;
using static Terwiel.Glaucon.Glaucon;

namespace Glaucon4Test.TestD
{
    public class UnitTestD
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
            ModalExaggeration = 20.0,
            XIncrement = -1,
            UnifLoadsLocal = true,
            // HTML = 1, Latex = 2, CSV = 3, Excel = 4, XML = 5
            OutputFormat = 2,
            RenumNodes = true,
            MaxSegmentCount = 20,
            MinimumIterations = 2,
            MaximumIterations = 13,
            AccountForShear = true,
            AccountForGeomStability = false,
            DeformationExaggeration = 20.0,
            Decimals = 2,
            ModalMethod = 1,
            Scale = 1.0,
            LumpedMassMatrix = true,
            DynamicModesCount = 14,
            PanRate = 2.0,
            Shift = 10.0,
            InputSource = 1
        };

        public Glaucon Glaucon = new()
        {

            Title = "Example D: dynamic properties of an un-restrained frame with a triangular section     ",

            Nodes = new List<Node>
            {
                new(  1,new double[]{ -120  ,  0    ,-210   },10  ),
                new(  1,new double[]{ -120  ,  0    ,-210   },10  ),
                new(  2,new double[]{      0,     0,    -210},  0 ),
                new(  3,new double[]{    120,     0,    -210},  10),
                new(  4,new double[]{     60,   115,    -210},  0 ),
                new(  5,new double[]{      0,   230,    -210},  10),
                new(  6,new double[]{    -60,   115,    -210},  0 ),
                new(  7,new double[]{  120  ,  0    , -90   },0   ),
                new(  8,new double[]{  120  ,  0    , -90   },0   ),
                new(  9,new double[]{    0  ,230    , -90   },0   ),
                new( 10,new double[]{ -120  ,  0    ,   0   },10  ),
                new( 11,new double[]{      0,     0,       0},  0 ),
                new( 12,new double[]{    120,     0,       0},  10),
                new( 13,new double[]{     60,   115,       0},  0 ),
                new( 14,new double[]{      0,   230,       0},  10),
                new( 15,new double[]{    -60,   115,       0},  0 ),
                new( 16,new double[]{ -120  ,  0    ,  90   },0   ),
                new( 17,new double[]{  120  ,  0    ,  90   },0   ),
                new( 18,new double[]{    0  ,230    ,  90   },0   ),
                new( 19,new double[]{ -120  ,  0    , 210   },10  ),
                new( 20,new double[]{      0,     0,     210},  0 ),
                new( 21,new double[]{    120,     0,     210},  10),
                new( 22,new double[]{     60,   115,     210},  0 ),
                new( 23,new double[]{      0,   230,     210},  10),
                new( 24,new double[]{    -60,   115,     210},  0 )
            },

            Members = new List<Member>
            {
                new( 1,  1,  2,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 2,  2,  3,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 3,  3,  4,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 4,  4,  5,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 5,  5,  6,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 6,  6,  1,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 7,  1,  7,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 8,  3,  8,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new( 9,  5,  9,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(10,  7, 10,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(11,  8, 12,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(12,  9, 14,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(13, 10, 11,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(14, 11, 12,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(15, 12, 13,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(16, 13, 14,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(17, 14, 15,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(18, 15, 10,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(19, 10, 16,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(20, 12, 17,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(21, 14, 18,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(22, 16, 19,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(23, 17, 21,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(24, 18, 23,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(25, 19, 20,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(26, 20, 21,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(27, 21, 22,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(28, 22, 23,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(29, 23, 24,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),
                new(30, 24, 19,new[]{  125.0,   100,   100},new[]{   5000.0,   2500,   2500}, new[]  {29000.0,   11500, 7.33e-7},0),


                },
            AnimatedModes = new[] { 1, 7, 9, 11, 12, 13 }


        };
    };
}


