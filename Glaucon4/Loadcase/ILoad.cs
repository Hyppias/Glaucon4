#region FileHeader
// Project: Glaucon4
// Filename:   ILoad.cs
// Last write: 4/30/2023 2:29:48 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        public partial class LoadCase
        {
            public interface ILoad
            {
                DenseVector GetLoadVector(Member mbr){ return null; }
            }
        }
    }
}
