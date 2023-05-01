#region FileHeader
// Project: Glaucon4
// Filename:   Animate.cs
// Last write: 4/30/2023 2:48:00 PM
// Creation:   4/24/2023 11:59:08 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader


using System;

using System.Diagnostics;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Double;

using System.IO;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        /// <summary>
        /// Animate the vibration mode
        /// </summary>
        /// <param name="lc">load case nr (base 0)</param>
        private void Animate(int lc)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);

            //string meshpath = $"{BaseFile}_anim_mesh_lc{lc}.plt";
            //string plotpath = $"{BaseFile}_anim_plot_{lc}.plt";
            //string modepath = $"{BaseFile}_mode_lc{lc}.plt";

            const int frames = 25;
            // int frameNumber = 0;
            //int m;
            const double
                rotXInit = 70.0, // inital x-rotation in 3D animation
                rotXFinal = 60.0, // final  x-rotation in 3D animation
                rotZInit = 100.0, // inital z-rotation in 3D animation
                rotZFinal = 120.0; // final  z-rotation in 3D animation
            double
                zoomInit = 1.0 * Param.Scale, // init.  zoom scale in 3D animation
                zoomFinal = 1.1 * Param.Scale; // final  zoom scale in 3D animation

            // determin containing box:
            var minXYZ = new DenseVector(3) { double.MaxValue, double.MaxValue, double.MaxValue };
            var maxXYZ = new DenseVector(3) { double.MinValue, double.MinValue, double.MinValue };
            foreach (var nd in Nodes)
            {
                for (var j = 0; j < 3; j++)
                {
                    minXYZ[j] = Math.Min(minXYZ[j], nd.Coord[j]);
                    maxXYZ[j] = Math.Max(maxXYZ[j], nd.Coord[j]);
                }
            }

            // box diagonal
            var Dxyz = Math.Sqrt(Sq(maxXYZ[0] - minXYZ[0]) + Sq(maxXYZ[1] - minXYZ[1]) + Sq(maxXYZ[2] - minXYZ[2]));

            // first add animation tot the main plot script:

            using (var script = new StreamWriter(Param.OutputPath + plotPath, true)) // true = append
            {
                script.WriteLine("\n# --- Mode shape animation ---");
                script.WriteLine($"rot_x_init  = {rotXInit:F2}");
                script.WriteLine($"rot_x_final = {rotXFinal:F2}");
                script.WriteLine($"rot_z_init  = {rotXInit:F2}");
                script.WriteLine($"rot_z_final = {rotZFinal:F2}");
                script.WriteLine($"zoom_init   = {zoomInit:F2}");
                script.WriteLine($"zoom_final  = {zoomInit:F2}");
                script.WriteLine($"#pan rate    = {Param.PanRate:F2}");
                script.WriteLine("set autoscale");
                script.WriteLine("unset border");
                if (Dim3)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        script.WriteLine($"unset {"xyz"[k]}label");
                    }

                    script.WriteLine("# unset label");
                    script.WriteLine("set parametric"); // Pas op: andersom?
                }

                script.WriteLine("unset key\n");

                for (var k = 0; k < 3; k++)
                {
                    var xyz = "xyz"[k];
                    script.WriteLine($"# {xyz}_min = {minXYZ[k]:F2}    {xyz}_max = {maxXYZ[k]:F2}");
                    script.WriteLine($"set {xyz}range [ {minXYZ[k] - 0.2 * Dxyz:F2} : {maxXYZ[k] + 0.1 * Dxyz:F2} ]");
                    script.WriteLine($"unset {xyz}zeroaxis; unset {xyz}tics;");
                }

                script.WriteLine($"# Containing box diagonal = {Dxyz:F2}");

                if (Dim3)
                {
                    script.WriteLine($"set view 60, 70, {Param.Scale}");
                }

                script.WriteLine("set size 1,1    # set the size so that the plot fills the available canvas:");
                if (Dim3)
                {
                    script.WriteLine("set view equal xyz # 1:1 3D axis scaling");
                }

                script.WriteLine("### key bind");

                script.WriteLine("bind \"x\" \"Stop = 1\"");

                script.WriteLine($"do for [m=1:{AnimationModes.Length}] " + "{"); // for each requested mode:
                script.WriteLine("Stop = 0");
                script.WriteLine("pause -1 sprintf(\"Start animation mode %d\",m)");
                script.WriteLine(
                    $"set title  tc rgb \"white\" sprintf(\"'{Title}     mode %d      %.2f Hz'\",Mn[m],F[m])");

                var totalFrames = frames;
                script.WriteLine("while (1) {");
                script.WriteLine($"do for [fr=0:{frames}] " + "{");

                script.WriteLine("if (Stop) { break }");

                //frameNumber++;

                script.Write($"{plot} '{meshPath}' using {D23} w l lt 1, ");
                script.WriteLine($"sprintf(\"{BaseFile}-mode%d.%d\",m,fr)  using {D12} w l lt 3");
                if (Param.PanRate != 0.0 && Dim3)
                {
                    script.WriteLine("set view " +
                        $"sprintf(\"%.2f , %.2f , %.2f\",{rotXInit} + ({Param.PanRate * (rotXFinal - rotXInit) / totalFrames}) * fr ," +
                        $"{rotZInit} + ({Param.PanRate * (rotZFinal - rotZInit) / totalFrames}) * fr, " +
                        $"{zoomInit} + ({Param.PanRate * (zoomFinal - zoomInit) / totalFrames}) * fr)" +
                        $"# pan = {Param.PanRate:F2}");
                }

                script.WriteLine("pause 0.05");

                script.WriteLine("}");
                // and break again to exit outer loop:3
                script.WriteLine("if (Stop) { break }");

                // and back:
                script.WriteLine($"do for [fr={frames - 1}:1:-1] " + "{");

                script.WriteLine("if (Stop) { break }");

                script.Write($"{plot} '{meshPath}' using {D23} w l lt 1, ");
                script.WriteLine($"sprintf(\"{BaseFile}-mode%d.%d\",m,fr)  using {D12} w l lt 3");
                if (Param.PanRate != 0.0 && Dim3)
                {
                    script.WriteLine("set view " +
                        $"sprintf(\"%.2f , %.2f , %.2f\",{rotXInit} + ({Param.PanRate * (rotXFinal - rotXInit) / totalFrames}) * fr , " +
                        $"{rotZInit} + ({Param.PanRate * (rotZFinal - rotZInit) / totalFrames}) * fr, " +
                        $"{zoomInit} + ({Param.PanRate * (zoomFinal - zoomInit) / totalFrames}) * fr)" +
                        $"# pan = {Param.PanRate:F2}");
                }

                script.WriteLine("pause 0.05");

                script.WriteLine("}");

                script.WriteLine("if (Stop) { break }");

                script.WriteLine("}\n}");

                //modefl = ModeFileName(BaseFile, m, fr);

                //script.Write($"{plot} '{meshPath}' using {D23} w l lw {lw} lt 5, ");
                //script.WriteLine($" '{modefl}' using {D12} w l lw 3 lt 3");				
                //script.WriteLine("}");
            } // end using file

            // now build the 25 individual frames for all modes:

            var v = new DenseVector(DoF);
            for (var i = 0; i < AnimationModes.Length; i++)
            {
                var m = AnimationModes[i];
                for (var fr = 0; fr <= frames; fr++)
                {
                    using (var script = new StreamWriter(Param.OutputPath + $"{BaseFile}-mode{i + 1}.{fr}"))
                    {
                        var ex = Param.ModalExaggeration * Math.Cos(Math.PI * fr / frames);

                        script.Write($"# {ProgramName} Analysis Results");
                        script.WriteLine($" Version {ProgramVersion}");
                        script.WriteLine($"# {Title}");
                        script.WriteLine("# Animated mode shape data");
                        script.WriteLine($"# deflection exaggeration: {ex:F2}");
                        script.WriteLine($"# Mode {m + 1}: f= {eigenFreq[m]:F2} Hz  T= {1d / eigenFreq[m]:F3} sec");

                        for (var j = 0; j < DoF; j++)
                        {
                            v[j] = Eigenvector[j, m]; /* mode "m" */
                        }

                        script.WriteLine("#      X-dsp       Y-dsp       Z-dsp");

                        foreach (var mbr in Members)
                        {
                            // mbr.CubicBentBeam(script, (DenseVector)LoadCases[lc].Displacements.Column(0));
                            mbr.CubicBentBeam(script, (DenseVector)Eigenvector.Column(m), ex);
                            script.WriteLine("\n");
                        }
                    } // end using script
                } // end frames
            } // end modes Anim

            Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
        }
    }
}
