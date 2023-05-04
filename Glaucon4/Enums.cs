#region FileHeader
// Project: Glaucon4
// Filename:   Enums.cs
// Last write: 4/30/2023 2:29:29 PM
// Creation:   4/27/2023 7:48:28 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

namespace Terwiel.Glaucon
{
    public enum Dir
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    public partial class Glaucon
    {
        const int RANGE = 3;
        const int SUBSPACE = 1;
        const int STODOLA = 2;
        const int ALL = 4;
        const int MKL = 5;
        const int FEAST = 6;


        const int HTML = 1;
        const int Latex = 2;
        const int CSV = 3;
        const int Excel = 4;
        const int XML = 5;

        const int None = 0;
        const int Static = 1;
        const int Modal = 3;
        const int Dynamic = 2; // PazGuyan
    }

}
