#region FileHeader
// Project: Glaucon4
// Filename:   TemperatureLoad.cs
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
    
    using MathNet.Numerics.LinearAlgebra.Double;

    public partial class Glaucon
    {
        public partial class LoadCase
        {
            /// <summary>
            /// The temperature load.
            /// <see cref="http://svn.code.sourceforge.net/p/frame3dd/code/trunk/doc/Frame3DD-manual.html"/>
            /// </summary>
            public class TempLoad : ILoad
            {
                public TempLoad(int mbr, double[] p, bool active = true)
                {
                    MemberNr = mbr - 1;
                    (alpha, hy, hz, tym, typ, tzm, tzp) = (p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
                    Active = active;
                }

                public bool Active;
                /// <summary>
                /// linear expansion coefficient
                /// </summary>
                public double alpha;

                /// <summary>
                /// hy depth.
                /// </summary>
                public double hy;

                /// <summary>
                /// hz depth.
                /// </summary>
                public double hz;

                /// <summary>
                /// Member nr.
                /// </summary>
                public int MemberNr;

                /// <summary>
                /// delta Ty - degrees.
                /// </summary>
                public double tym;

                /// <summary>
                /// delta Ty +.
                /// </summary>
                public double typ;

                /// <summary>
                /// delta Tz -.
                /// </summary>
                public double tzm;

                /// <summary>
                /// delta Tz +.
                /// </summary>
                public double tzp;

                public TempLoad()
                {
                }

                //public TempLoad(int memberNr)
                //{
                //    nr = memberNr;
                //    alpha = buffer.ReadDouble(); // 2
                //    hy = buffer.ReadDouble(); // 3
                //    hz = buffer.ReadDouble(); // 4
                //    typ = buffer.ReadDouble(); // 5
                //    tym = buffer.ReadDouble(); // 6
                //    tzp = buffer.ReadDouble(); // 7
                //    tzm = buffer.ReadDouble(); // 8
                //}

                /// <summary>
                /// get/build the  thermal load vector.
                /// </summary>
                /// <Param name="mbr">
                /// The member number, base 0.
                /// </Param>
                /// <returns>
                /// The <see cref="DenseVector">the fixed end forces</see>.
                /// </returns>
                public DenseVector GetLoadVector(Member mbr)
                {
                    if (mbr == null)
                    {
                        return null;
                    }

                    double f6 = alpha * (1.0 / 4.0) * (typ + tym + tzp + tzm) * mbr.Mat.E
                                * mbr.As[0],
                           f4 = (alpha / hz) * (tzm - tzp) * mbr.Mat.E * mbr.Iz[1],
                           f5 = (alpha / hy) * (typ - tym) * mbr.Mat.E * mbr.Iz[1];
                    var fixedEndForces = Vector.Build.DenseOfArray(new[] { -f6, 0, 0, 0, f4, f5, f6, 0, 0, 0, -f4, -f5, });
                    return (DenseVector)fixedEndForces;

                    // F[6] = alpha * (1.0 / 4.0) * (typ + tym + tzp + tzm) * mbr.Material.E * mbr.Area;
                    // F[0] = -F[6];

                    // F[4] = (alpha / hz) * (tzp + tzm) * mbr.Material.E * mbr.IyZ;
                    // F[10] = -F[4];
                    // F[5] = (alpha / hy) * (typ + tym) * mbr.Material.E * mbr.IxZ;
                    // F[11] = -F[5];
                    // return F;
                }

               
            }
        }
    }
}