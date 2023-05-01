#region FileHeader
// Project: Glaucon4
// Filename:   PrescrDispl.cs
// Last write: 4/30/2023 2:29:48 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        public partial class LoadCase
        {
            public class PrescrDisplacement
            {
                public PrescrDisplacement(int nd, double[] disp, bool active = true)
                {
                    NodeNr = nd-1;
                    Displacements = disp;
                }
                public bool Active;
                public int NodeNr;
                public double[] Displacements;
            }
        }
    }
}
