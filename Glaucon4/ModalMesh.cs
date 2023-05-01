#region FileHeader
// Project: Glaucon4
// Filename:   ModalMesh.cs
// Last write: 4/30/2023 2:29:35 PM
// Creation:   4/24/2023 12:01:09 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.Diagnostics;
using System.IO;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        private void ModalMesh()
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);
            var ms = new DenseMatrix(DoF, 3);
            //DenseVector v = new DenseVector(DoF);
            var ModalPartFactor = new DenseVector(3);
            //string modeFl;

            // Modal Particiation factors:
            for (var i = 0; i < DoF; i++)
                for (var k = 0; k < 3; k++)
                    for (var j = k; j < DoF; j += 6)
                    {
                        ms[i, k] += M[i, j];
                    }

            if (!Param.Analyze)
            {
                Param.ModalExaggeration = 1f;
            }

            // Plot all modal meshes:

            for (var m = 0; m < Param.DynamicModesCount; m++)
            {
                // These scripts are called from the control script 'plotPath'
                using (var script = new StreamWriter($"{Param.OutputPath}{BaseFile}_Mode_{m + 1}"))
                {
                    script.Write($"# {ProgramName} Analysis Results ");
                    script.WriteLine($" Version {ProgramVersion}");
                    script.WriteLine($"# {Title}");
                    script.WriteLine($"# Mode shape data for mode {m + 1} (global coordinates)");
                    script.WriteLine($"# deflection exaggeration: {(double) Param.ModalExaggeration:F2}\n");
                    for (var j = 0; j < 3; j++)
                    {
                        ModalPartFactor[j] = 0.0;
                        for (var i = 0; i < DoF; i++)
                        {
                            ModalPartFactor[j] += Eigenvector[i, m] * ms[i, j];
                        }
                    }

                    script.WriteLine($"# Mode {m + 1}:   f= {eigenFreq[m]:F2} Hz, T= {1d / eigenFreq[m]:F3} sec");
                    script.WriteLine("# Modal participation factors:\n" +
                        "#  – modal participation factors are scalars that measure the interaction\n" +
                        "#    between the modes and the directional excitation in a given reference frame.\n" +
                        "#    Larger values indicate a stronger contribution to the dynamic response.");
                    for (var j = 0; j < 3; j++)
                    {
                        script.WriteLine($"#\t\t{"XYZ"[j]}- modal participation factor = {ModalPartFactor[j]:E3}");
                    }

                    //for (int i = 0; i < DoF; i++)
                    //	v[i] = Eigenvector[i, m]; // Eigenvalues
                    script.WriteLine("#      X-dsp       Y-dsp       Z-dsp");
                    foreach (var mbr in Members)
                    {
                        mbr.CubicBentBeam(script, (DenseVector)Eigenvector.Column(m), Param.DeformationExaggeration);
                        script.WriteLine("\n"); // two newlines to separate members!!
                    }
                } // end using
            }

            // Add modal mesh plotting to the main plot script:

            using (var script = new StreamWriter(Param.OutputPath + plotPath, true))
            {
                script.WriteLine("\n# ===== show the modal meshes for all load cases ======\n");
                script.WriteLine("# unset label # do (not) show labels");
                if (Dim3)
                {
                    script.WriteLine("unset key");
                }

                script.Write($"array F[{AnimationModes.Length}] = [");
                for (var i = 0; i < AnimationModes.Length; i++)
                {
                    script.Write($"{eigenFreq[i]:F2}, ");
                }

                script.WriteLine(" ]\n");
                script.Write($"array Mn[{AnimationModes.Length}] = [");
                foreach (var a in AnimationModes) // (int i = 0; i < Anim.Length; i++)
                {
                    script.Write($"{a:F2}, ");
                }

                script.WriteLine(" ]\n");

                script.WriteLine($"do for [i = 1:{AnimationModes.Length}] " + "{");
                script.WriteLine("pause -1 sprintf(\"Mode %d\", i)");
                script.WriteLine($"set title  tc rgb \"white\" sprintf(\"{BaseFile} mode %d  %.2f Hz\", Mn[i],F[i])");

                script.Write($"{plot} '{meshPath}' using {D23} t 'undeformed mesh' w l ");

                script.WriteLine(!Param.Analyze
                    ? " lt 1"
                    : $" lt 2 , sprintf(\"{BaseFile}_Mode_%d\",i) using {D12} t sprintf(\"'mode-shape %d'\", i) w l lt 3");

                script.WriteLine("}"); // end do for
            }

            Debug.WriteLine("Exit" + MethodBase.GetCurrentMethod().Name);
        } // end ModalMesh
    }
}
