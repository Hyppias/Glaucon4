#region FileHeader
// Project: Glaucon4
// Filename:   IntPointLoad.cs
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
            /// <summary>
            /// A point load along the member.
            /// This iis NOT the load on a node.
            /// <see cref="http://svn.code.sourceforge.net/p/frame3dd/code/trunk/doc/Frame3DD-manual.html"/>
            /// </summary>
            public class IntPointLoad : ILoad
            {
                public IntPointLoad(int mbr,  double[] load, double pos, bool active = true)
                {
                    MemberNr = mbr-1;
                    Load = load;
                    Position = pos;
                    Active = active;

                }

                public bool Active;

                /// <summary>
                /// three components of the point load: X, Y ans Z.
                /// </summary>
                public DenseVector Load; // size 3

                /// <summary>
                /// The member nr that the load is on.
                /// </summary>
                public int MemberNr;

                /// <summary>
                /// The position of the load along the member.
                /// </summary>
                public double Position;

                public DenseVector GetLoadVector(Member mbr)
                {
                    var Ksz = mbr.Ksz;
                    var Ksy = mbr.Ksy;
                    var Ln = mbr.Length;

                    var a = Position;
                    var b = Ln - a;
                    var fixedEndForces = Vector.Build.DenseOfArray(
                        new[]
                            {
                            (Load[0] * a) / Ln,
                            (((1.0 / (1.0 + Ksz)) * Load[1] * b * b * ((3.0 * a) + b)) / (Ln * Ln * Ln))
                            + (((Ksz / (1.0 + Ksz)) * Load[1] * b) / Ln),
                            (((1.0 / (1.0 + Ksy)) * Load[2] * b * b * ((3.0 * a) + b)) / (Ln * Ln * Ln))
                            + (((Ksy / (1.0 + Ksy)) * Load[2] * b) / Ln),
                            0.0,
                            ((-(1.0 / (1.0 + Ksy)) * Load[2] * a * b * b) / (Ln * Ln))
                            - (((Ksy / (1.0 + Ksy)) * Load[2] * a * b) / (2.0 * Ln)),
                            (((1.0 / (1.0 + Ksz)) * Load[1] * a * b * b) / (Ln * Ln))
                            + (((Ksz / (1.0 + Ksz)) * Load[1] * a * b) / (2.0 * Ln)),
                            (Load[0] * b) / Ln,
                            (((1.0 / (1.0 + Ksz)) * Load[1] * a * a * ((3.0 * b) + a)) / (Ln * Ln * Ln))
                            + (((Ksz / (1.0 + Ksz)) * Load[1] * a) / Ln),
                            (((1.0 / (1.0 + Ksy)) * Load[2] * a * a * ((3.0 * b) + a)) / (Ln * Ln * Ln))
                            + (((Ksy / (1.0 + Ksy)) * Load[2] * a) / Ln),
                            0.00,
                            (((1.0 / (1.0 + Ksy)) * Load[2] * a * a * b) / (Ln * Ln))
                            + (((Ksy / (1.0 + Ksy)) * Load[2] * a * b) / (2.0 * Ln)),
                            ((-(1.0 / (1.0 + Ksz)) * Load[1] * a * a * b) / (Ln * Ln))
                            - (((Ksz / (1.0 + Ksz)) * Load[1] * a * b) / (2.0 * Ln)),
                            });
                    return (DenseVector)fixedEndForces;
                }
            }
        }
    }
}
