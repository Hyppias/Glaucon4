#region FileHeader
// Project: Glaucon4Test
// Filename:   TestCondensation.cs
// Last write: 5/3/2023 3:38:17 PM
// Creation:   4/24/2023 12:39:30 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using gl = Terwiel.Glaucon;

namespace UnitTestGlaucon
{
    public partial class UnitTestGlaucon2
    {
        [TestMethod]
        public void TestGavinModalCond()
        {
            var TestObject = new TestEobject();
            var param = TestObject.Param;
            var glaucon = TestObject.Glaucon;
            var result = TestObject.Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            
            var KcSoll = DenseMatrix.Build.DenseOfArray(new[,]
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
            var TestObject = new TestEobject();
            var param = TestObject.Param;
             param.CondensationMethod = 1;
            var glaucon = TestObject.Glaucon;
            var result = TestObject.Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            result = glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            
            var KcSoll = DenseMatrix.Build.DenseOfArray(new[,]
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
           var TestObject = new TestEobject();
            var param = TestObject.Param;
             param.CondensationMethod = 2; // dynamic
            var glaucon = TestObject.Glaucon;
            var result = TestObject.Glaucon.Execute(ref deflection, ref Reactions, ref EndForces);
            foreach (var e in gl.Glaucon.Errors) //for (int i = 0; i < gl.Glaucon.Errors.Count; i++)
                Debug.WriteLine(e);
            var KcSoll = DenseMatrix.Build.DenseOfArray(new[,]
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
