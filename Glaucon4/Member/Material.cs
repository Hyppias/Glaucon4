#region FileHeader
// Project: Glaucon4
// Filename:   Material.cs
// Last write: 4/30/2023 2:29:47 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        public partial class Member
        {
            public class Material
            {
                public Material(double e, double g, double r, double a, bool active = true)
                {
                    E = e;
                    G = g;
                    Density = r;
                    Alpha = a;
                    Active = active;
                }

                public bool Active;
                [XmlAttribute("E"), JsonProperty("E")]
                [Description("Elastic modulus")]
                // input in N/square mm
                public double E { get; set; }

                [XmlAttribute("G"), JsonProperty("G")]
                [Description("Shear modulus")]
                // input in N/square mm
                public double G { get; set; }

                [XmlAttribute("Density"), JsonProperty("Density")]
                [Description("Density")]
                // Density to be input in tonne/cubic mm:
                // so: input 7800 kilogram/cubic meter as 7.8e-9 (tonne/cubic mm)
                public double Density { get; set; }

                [XmlAttribute("Alpha"), JsonProperty("Alpha")]
                [Description("Linear expansion coefficient")]
                // input in m/(m.K)
                public double Alpha { get; set; }
            }
        }
    }
}
