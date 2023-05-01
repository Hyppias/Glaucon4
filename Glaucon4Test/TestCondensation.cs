#region FileHeader

// Solution: Glaucon
// Project: UnitTestGlaucon2
// Filename: TestCondensation.cs
// Date: 2021-09-08
// Created date: 2020-02-05
// Created time:-1:51 PM
// 
// Copyright: E.H. Terwiel, 2021, the Netherlands
// 
// No part of these files may be copied in any form without written consent
// of the programmer/owner/copyrightholder.

#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using dbl = MathNet.Numerics.LinearAlgebra.Double;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestGlaucon2
    {
        [TestMethod]
        public void TestGavinModalCond()
        {
            param.InputFileName = "exE.3dd";
            var result = ReadFile(param.InputPath + param.InputFileName);
            Assert.AreEqual(result, 0, $"Error reading {param.InputFileName}");

            var glaucon = new gl.Glaucon(ms.GetBuffer(), param)
            {
                CondensationMethod = gl.CondenseMode.Dynamic
            };
            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            Assert.AreEqual(result, 0, $"Error executing glaucon {param.InputFileName}");
            var KcSoll = dbl.DenseMatrix.Build.DenseOfArray(new[,]
            {
                {6.023820364848e+01, 3.501783885396e-02, 8.485744892121e+02},
                {3.501783885396e-02, 6.105274879932e+01, -3.183014038090e+02},
                {8.485744892121e+02, -3.183014038090e+02, 2.840638035507e+05}
            });
            CheckMatrix(glaucon.Kc, KcSoll, 3, "Condensed stiffness matrix (Modal).");
            // CheckMatrix(Glaucon.Mc, McSoll, 7, "Condensed mass matrix (Modal).");
        }

        [TestMethod]
        public void TestGavinStaticCond()
        {
            param.InputFileName = "exE.3dd";
            var result = ReadFile(param.InputPath + param.InputFileName);
            Assert.AreEqual(result, 0, $"Error reading {param.InputFileName}");

            var glaucon = new gl.Glaucon(ms.GetBuffer(), param)
            {
                CondensationMethod = gl.CondenseMode.Static
            };

            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            Assert.AreEqual(result, 0, $"Error executing glaucon {param.InputFileName}");

            var KcSoll = dbl.DenseMatrix.Build.DenseOfArray(new[,]
            {
                {6.023820364848e+01, 3.501783885396e-02, 8.485744892121e+02},
                {3.501783885396e-02, 6.105274879932e+01, -3.183014038090e+02},
                {8.485744892121e+02, -3.183014038090e+02, 2.840638035507e+05}
            });

            CheckMatrix(glaucon.Kc, KcSoll, 3, "Condensed stiffness matrix. (Static) ");
        }

        [TestMethod]
        public void TestGavinPazCond()
        {
            param.InputFileName = "exE.3dd";
            var result = ReadFile(param.InputPath + param.InputFileName);
            Assert.AreEqual(result, 0, $"Error reading {param.InputFileName}");

            var glaucon = new gl.Glaucon(ms.GetBuffer(), param)
            {
                CondensationMethod = gl.CondenseMode.PazGuyan
            };
            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);

            var KcSoll = dbl.DenseMatrix.Build.DenseOfArray(new[,]
            {
                {6.022989225039E+001, 3.696068438625E-002, 8.023513500879E+002},
                {3.696068438625E-002, 6.105421365455E+001, -3.358525342682E+002},
                {8.023513500879E+002, -3.358525342682E+002, 2.791496528963E+005}
            });

            CheckMatrix(glaucon.Kc, KcSoll, 3, "Condensed stiffness matrix. (Paz/Guyan) ");
            // CheckMatrix(Glaucon.Mc, McSoll, 7, "Condensed mass matrix (Paz/Guyan).");
        }
    }
}