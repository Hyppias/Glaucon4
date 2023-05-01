#region FileHeader
// Project: Glaucon4
// Filename:   TempLoad.cs
// Last write: 4/30/2023 2:29:42 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader


using System.ComponentModel;
using System.Xml.Serialization;
using dbl = MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
//using System.Json;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {

        public class LoadCase
        {
            //[Serializable]
            public class TempLoad
            {
                public TempLoad()
                {
                }

                public TempLoad(int n, float alpha, float hy, float hz, float typ, float tym, float tzp, float tzm)
                {
                    // TODO: check if maybe -1?
                    Nr = n; // No: Is OK
                    Alpha = alpha; //2
                    Hy = hy; //3
                    Hz = hz; //4
                    Typ = typ; //5
                    Tym = tym; //6
                    Tzp = tzp; //7
                    Tzm = tzm; //8
                }

                [XmlAttribute("Nr"), JsonProperty("Nr")]
                [Description("number of the heated/cooled member")]
                public int Nr { get; }

                [XmlAttribute("Alpha"), JsonProperty("Alpha")]
                [Description("Linear expansion coëfficiënt")]
                public float Alpha { get; }

                [XmlAttribute("hy"), JsonProperty("hy")]
                [Description("Cross section dimension (local coord.): Y direction")]
                public float Hy { get; set; }

                [XmlAttribute("hz"), JsonProperty("hz")]
                [Description("Cross section dimension (local coord.): Z direction")]
                public float Hz { get; }

                [XmlAttribute("Typ"), JsonProperty("Typ")]
                public float Typ { get; }

                [XmlAttribute("Tym"), JsonProperty("Tym")]
                public float Tym { get; }

                [XmlAttribute("Tzp"), JsonProperty("Tzp")]
                public float Tzp { get; }

                [XmlAttribute("Tzm"), JsonProperty("Tzm")]
                public float Tzm { get; }

                // Temperature changes cause a changed in length, therefor an extra force.
                // This force vector is computed here:
                public dbl.DenseVector GetLoadVector(Member mbr)
                {
                    double
                        f6 = Alpha * (1.0 / 4.0) * (Typ + Tym + Tzp + Tzm) * mbr.Mat.E * mbr.As[0],
                        f4 = Alpha / Hz * (Tzm - Tzp) * mbr.Mat.E * mbr.Iz[1],
                        f5 = Alpha / Hy * (Typ - Tym) * mbr.Mat.E * mbr.Iz[2];
                    var fixedEndForces = dbl.Vector.Build.DenseOfArray(
                        new[] { -f6, 0, 0, 0, f4, f5, f6, 0, 0, 0, -f4, -f5 });
                    return (dbl.DenseVector)fixedEndForces;
                }
            }
        }
    }
}
