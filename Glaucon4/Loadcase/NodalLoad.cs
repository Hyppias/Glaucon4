#region FileHeader
// Project: Glaucon4
// Filename:   NodalLoad.cs
// Last write: 4/30/2023 2:29:42 PM
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
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        public partial class LoadCase
        {
            [Serializable]
            public class NodalLoad
            {
                public NodalLoad(int n, double[] load, bool active = true)
                {
                    NodeNr = n - 1; // Node Nr. base 0
                    Load = DenseVector.OfArray(load);
                    Active = active;
                }

                [XmlAttribute("Nr"), JsonProperty(nameof(NodeNr))]
                [Description("number of the nodal load")]
                public int NodeNr { get; set; } // Node  Nr. base 0

                [XmlVector("Load"), JsonProperty(nameof(Load))]
                [Description("6 (XYZ) components (Forces + Moments) of the nodal load")]
                public DenseVector Load { get; set; } = new DenseVector(6);

                public bool Active;
            }
        }
    }
}
