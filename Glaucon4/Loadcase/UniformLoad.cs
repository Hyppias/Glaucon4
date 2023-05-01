#region FileHeader
// Project: Glaucon4
// Filename:   UniformLoad.cs
// Last write: 4/30/2023 2:30:48 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.ComponentModel;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        public partial class LoadCase
        {
            [Serializable]
            public class UniformLoad : ILoad
            {
                public UniformLoad(int memberNr, double[] q, bool active = true)
                {
                    MemberNr = memberNr - 1; // Member Nr. base 0

                    this.Q = new DenseVector(q);
                    Active = active;

                }
                public bool Active;
                [XmlAttribute("Nr"), JsonProperty("Nr")]
                [Description("number of the uniform load")]
                public int MemberNr { get; set; } // Member Nr. base 0

                [XmlVector("qLoad"), JsonProperty("qLoad")]
                [Description("3 (XYZ) components of the uniform load")]
                public DenseVector Q { get; set; }

                public DenseVector GetLoadVector(Member mbr)
                {
                    var fixedEndForces = new DenseVector(12);
                    //var mbr = mbrs[n = uLoad.MemberNr];
                    var effLength = mbr.Le;

                    // First in local member coordinates:
                    fixedEndForces[0] = fixedEndForces[6] = Q[0] * effLength / 2.0;
                    fixedEndForces[1] = fixedEndForces[7] = Q[1] * effLength / 2.0;
                    fixedEndForces[2] = fixedEndForces[8] = Q[2] * effLength / 2.0;

                    // fixedEndForces[3] = fixedEndForces[9] = 0.0;
                    fixedEndForces[10] = -(fixedEndForces[4] = -Q[2] * effLength * effLength / 12.0);
                    fixedEndForces[11] = -(fixedEndForces[5] = Q[1] * effLength * effLength / 12.0);
                    return fixedEndForces;
                }

            }
        }
    }
}
