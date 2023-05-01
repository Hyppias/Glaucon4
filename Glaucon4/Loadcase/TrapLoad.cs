#region FileHeader
// Project: Glaucon4
// Filename:   TrapLoad.cs
// Last write: 4/30/2023 2:29:44 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
namespace Terwiel.Glaucon
{


    public partial class Glaucon
    {
        public partial class LoadCase
        {
            [Serializable]
            public class TrapLoad
            {
                public TrapLoad(int memberNr, double[][] loads)
                {
                    MemberNr = memberNr - 1; // Member Nr. base 0

                    for (var i = 0; i < 3; i++) // three directions
                    {
                        Loads[i] = new Load() { a = loads[i][0], b = loads[i][1], Wa = loads[i][2], Wb = loads[i][3] };
                        //{
                        //    a = buffer.ReadSingle(),
                        //    b = buffer.ReadSingle(),
                        //    Wa = buffer.ReadSingle(),
                        //    Wb = buffer.ReadSingle()
                        //};
                    }
                }

                [XmlAttribute("MbrNr"), JsonProperty("MbrNr")]
                [Description("Number of the member the trapezium load acts on")]
                public int MbrNr { get; } // Member Nr. base 0

                [XmlArray("Loads")]
                [XmlArrayItem("Load")]
                [Description("3 (XYZ) Trapezoidal Loads")]
                public Load[] Loads { get; set; } = new Load[3];

                [Serializable]
                public class Load
                {
                    [XmlAttribute("a"), JsonProperty("a")]
                    [Description("Start position of the load")]
                    public double a { get; set; }

                    [XmlAttribute("b"), JsonProperty("b")]
                    [Description("End position of the load")]
                    public double b { get; set; }

                    [XmlAttribute("Wa"), JsonProperty("Wa")]
                    [Description("Intensity of the load at the start (a) position")]
                    public double Wa { get; set; }

                    [XmlAttribute("Wb"), JsonProperty("Wb")]
                    [Description("Intensity of the load at the end (b) position")]
                    public double Wb { get; set; }

                    public void Build2(double Ln, ref double R1o, ref double R2o, ref double f01, ref double f02)
                    {
                        double a2, a4, b2, b4, ab, waWb;
                        R1o = ((2.0 * Wa + Wb) * (a2 = a * a) - (Wa + 2.0 * Wb) * (b2 = b * b) +
                            3.0 * (Wa + Wb) * Ln * (b - a) - (waWb = Wa - Wb) * a * b) / (6.0 * Ln);
                        R2o = ((Wa + 2.0 * Wb) * b2 + waWb * a * b -
                            (2.0 * Wa + Wb) * a2) / (6.0 * Ln);

                        f01 = (3.0 * (Wb + 4.0 * Wa) * (a4 = a2 * a2) - 3.0 * (Wa + 4.0 * Wb) * (b4 = b2 * b2)
                            - 15.0 * (Wb + 3.0 * Wa) * Ln * a2 * a + 15.0 * (Wa + 3.0 * Wb) * Ln * b2 * b
                            - 3.0 * waWb * (ab = a * b) * (a2 + b2)
                            + 20.0 * (Wb + 2.0 * Wa) * Ln * Ln * a2 - 20.0 * (Wa + 2.0 * Wb) * Ln * Ln * b2
                            + 15.0 * waWb * Ln * ab * (a + b)
                            - 3.0 * waWb * a2 * b2 - 20.0 * waWb * Ln * Ln * a * b) / 360.0;

                        f02 = (3.0 * (Wb + 4.0 * Wa) * a4 - 3.0 * (Wa + 4.0 * Wb) * b4
                            - 3.0 * waWb * ab * (a2 + b2)
                            - 10.0 * (Wb + 2.0 * Wa) * Ln * Ln * a2 + 10.0 * (Wa + 2.0 * Wb) * Ln * Ln * b2
                            - 3.0 * waWb * a2 * b2 + 10.0 * waWb * Ln * Ln * a * b) / 360.0;
                    }

                    public void Build1(double Ln, ref double f01, ref double f02)
                    {
                        double b2, a2;
                        f01 = (3.0 * (Wa + Wb) * Ln * (b - a) - (2.0 * Wb + Wa) * (b2 = b * b) + (Wb - Wa) * b * a +
                            (2.0 * Wa + Wb) * (a2 = a * a)) / (6.0 * Ln);
                        f02 = (-(2.0 * Wa + Wb) * a2 + (2.0 * Wb + Wa) * b2 - (Wb - Wa) * a * b) / (6.0 * Ln);
                    }
                }
            }
        }
    }
}
