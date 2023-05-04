#region FileHeader
// Project: Glaucon4
// Filename:   StaticMesh.cs
// Last write: 4/30/2023 2:29:40 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Double;


namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        private bool Dim3;
        private int lw = 2; // line width GNUPlot

        private string plot = "plot", // 2D plot command
            D12 = "1:2", // use column 1 and 2 from the mesh file for 2D
            D23 = "2:3";

        private string plotPath,
            meshPath; // Undeformed mesh

        private void InitStaticMesh()
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            plotPath = $"{BaseFile}_plot.plt"; // the main plot file
            meshPath = $"{BaseFile}_mesh.plt"; // undeformed mesh
            lw = 1; // line width of deformed mesh

            // write gnuplot plotting script commands

            Dim3 = Space.Length == 3;

            // Build the Control script.
            // write header, plot-setup cmds, node label, and element label data			

            using (var script = new StreamWriter($"{Param.OutputPath}{plotPath}", false))
            {
                // header & node number & element number labels

                script.Write(
                    $"# {ProgramName} Analysis Results. " +
                    $"version {ProgramVersion}" +
                    $"# {Title}" +
                    $"# Date/time: {DateTime.Now}" +
                    "# GNUPlot 5.2 script file" +

                    "set autoscale" +
                    "unset border" +
                    "set pointsize 1.0" +
                    "set xtics; set ytics; set ztics;" +
                    "unset zeroaxis" +
                    "unset key" +
                    "unset label" +

                    "set size 1,1    # 1:1 2D axis scaling " +
                    "set view equal xyz # 1:1 3D axis scaling " +

                    "set terminal windows background rgb \"black\"" +
                    $"set linetype 1 lc rgb \"green\" lw {lw} pt 1" +
                    $"set linetype 2 lc rgb \"red\" lw {lw} pt 7" +
                    $"set linetype 3 lc rgb \"yellow\" lw {lw} pt 6 pi-1" +
                    $"set linetype 4 lc rgb \"black\" lw {lw} pt 5 pi-1" +
                    //script.WriteLine($"set tc rgb \"white\"");
                    "\n# ===== Node number labels ====="
                );
                foreach (var node in Nodes)
                {
                    var c = node.Coord;
                    // tc = textcolor
                    script.WriteLine($"set label ' {node.Nr + 1}' tc rgb \"white\" at {c[0]}, {c[1]}, {c[2]}");
                }

                script.WriteLine("\n# ===== Element number labels =====");
                foreach (var mbr in Members)
                {
                    // tc = text color
                    var m = (mbr.NodeA.Coord + mbr.NodeB.Coord) * 0.5f; // half way
                    script.WriteLine($"set label ' {mbr.Nr + 1}' tc rgb \"white\" at {m[0]}, {m[1]}, {m[2]}");
                }

                // 3D plot setup commands
                script.WriteLine(
                    "\n# ====== Show, for all load cases, the deformed and the undeformed construction ======\n");
                if (Dim3)
                {
                    plot = "splot"; // 3D plot command
                    D12 = "1:2:3"; // use all 3 coordinate columns
                    D23 = "2:3:4";
                    script.WriteLine("set parametric"+
                    $"set view 60, 70, {Param.Scale:F3}"+
                    "set view equal xyz # 1:1 3D axis scaling"+
                    "unset key");
                    for (var k = 0; k < 3; k++)
                    {
                        script.WriteLine($"set {"xyz"[k]}label '{"xyz"[k]}'");
                    }
                }

                //	 script.WriteLine($"%c unset label\n", D3 );
                if (Param.Analyze)
                {
                    script.WriteLine($"do for [i=1:{LoadCases.Count}] " + "{");
                    script.WriteLine("pause -1 sprintf(\"Show load case %d\",i)");

                    script.Write($"set title tc rgb \"white\" sprintf(\"{Title}, Analysis file: {BaseFile}, ");
                    if (Param.Analyze)
                    {
                        script.WriteLine(Param.Analyze
                            ? $"deflection exaggeration: {Param.DeformationExaggeration}, load case %d of {LoadCases.Count}\",i)"
                            : "data check only\"");
                    }

                    script.WriteLine("unset clip; \nset clip one; set clip two");
                    script.WriteLine("set xyplane 0"); // requires Gnuplot >= 4.6					
                                                       // t = title
                    script.Write($"{plot} '{meshPath}' using {D23} t 'undeformed mesh' w lp ");
                    // lt = line type, lw = line width, w = with, l = line
                    script.WriteLine(!Param.Analyze
                        ? "lt 1"
                        : $"lt 2 , sprintf(\"{BaseFile}_lc_%d\",i) using {D12} t sprintf(\"'load case %d of {LoadCases.Count}'\",i)" +
                        $" w l lw {lw} lt 3");

                    // script.WriteLine("pause -1");
                    script.WriteLine("}");
                }
            }

            // write undeformed mesh data to its own file:
            using (var undef = new StreamWriter($"{Param.OutputPath}{meshPath}", false)) // false  = no append
            {
                undef.WriteLine($"# {ProgramName} Analysis Results version {ProgramVersion}");
                undef.WriteLine($"# {Title}");
                undef.WriteLine($"# Date/time: {DateTime.Now}");
                undef.WriteLine("# Undeformed mesh data (global coordinates)");
                undef.WriteLine("# Node  X  Y  Z");

                foreach (var mbr in Members)
                {
                    var CA = mbr.NodeA.Coord;
                    var CB = mbr.NodeB.Coord;
                    undef.WriteLine($"{mbr.NodeA.Nr + 1} {CA[0]} {CA[1]} {CA[2]}");
                    undef.WriteLine($"{mbr.NodeB.Nr + 1} {CB[0]} {CB[1]} {CB[2]}\n\n");
                }
            }

            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// write deformed mesh procedure to GNUPlot script
        /// </summary>
        /// <param name="lc">Load case number to plot</param>
        private void DeformedMeshLC(LoadCase lc)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            using (var defrm = new StreamWriter($"{Param.OutputPath}{BaseFile}_lc_{lc.Nr + 1}"))
            {
                // write deformed mesh data

                defrm.WriteLine($"# {ProgramName} Analysis Results, version {ProgramVersion}");
                defrm.WriteLine($"# {Title}");
                defrm.WriteLine($"# Load case {lc.Nr + 1} of {LoadCases.Count}");
                defrm.WriteLine($"# Date/time: {DateTime.Now}");
                defrm.Write($"# Deformed mesh data    deflection exaggeration: {Param.DeformationExaggeration}");
                defrm.WriteLine("#       X-dsp        Y-dsp        Z-dsp");

                foreach (var mbr in Members)
                {
                    // write deformed shape data for each element					
                    if (mbr.XIncrementCount > 0.0 && lc.TransvDispl?[mbr.Nr].RowCount > 0)
                    {
                        // write the deformed shape on the basis of the calculated
                        // inter-node deformations (see Peak)
                        mbr.ForceBentBeam(defrm, lc.TransvDispl[mbr.Nr]);
                    }
                    else
                    {
                        // fallback:
                        // no transversal displacements were calculated, so
                        // write the deformed shape on the basis of the member end (node) forces/deformations
                        mbr.CubicBentBeam(defrm, (DenseVector)lc.Displacements.Column(0), Param.DeformationExaggeration);
                    }

                    defrm.WriteLine("\n");
                }
            }

            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        } // End StaticMesh
    }
}
