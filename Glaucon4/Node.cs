#region FileHeader
// Project: Glaucon4
// Filename:   Node.cs
// Last write: 4/30/2023 2:29:48 PM
// Creation:   4/24/2023 11:59:09 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using MathNet.Numerics.LinearAlgebra.Double;

using System.ComponentModel;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        /// <summary>
        /// A point in space
        /// </summary>
        //public readonly struct Point3D : IEquatable<Point3D>
        //{
        //    public double X { get; }

        //    public double Y { get; }

        //    public double Z { get; }

        //    public Point3D(double x, double y, double z)
        //    {
        //        X = x;
        //        Y = y;
        //        Z = z;
        //    }

        //    public override bool Equals(object? obj) => throw new NotImplementedException();

        //    public override int GetHashCode() => throw new NotImplementedException();
        //    public bool Equals(Point3D other) => throw new NotImplementedException();

        //    public static bool operator ==(Point3D left, Point3D right)
        //    {
        //        return left.Equals(right);
        //    }

        //    public static bool operator !=(Point3D left, Point3D right)
        //    {
        //        return !(left == right);
        //    }

        //    public static bool operator -(Point3D left, Point3D right)
        //    {
        //        return Math.Sqrt(Sq(left.X - right.X) +  Sq(left.Y - right.Y) +Sq(left.Z - right.Z));
        //    }
        //}

        public partial class Node
        {
            /// <summary>
            /// an array of six booleans, signifying the freedom in a specific direction.
            /// TODO: maybe not necessary?
            /// </summary>
            public int[] Restraints;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            public Node(int nr, double[] coord, double radius , bool active = true)
            {
                Nr = nr;
                Coord= new DenseVector(coord);
                NodeRadius = radius;
                Active = true; // nodes may never be made inactive.
            }

            public bool Active { get; set; }

            public DenseVector Coord { get; set; }

            public DenseVector ExtraNodalMass { get; set; } // extra nodal  mass

            //[Description("Extra node mass"),]
            //public double NM { get; set; }

            [Description("node size radius, for finite sizes "),]
            public double NodeRadius { get; set; }

            /// <summary>
            /// Gets the number of the node.
            /// </summary>
            public int Nr { get; set; }

            /// <summary>
            /// add node mass to mass matrix.
            /// </summary>
            public void AddMassToMassMatrix()
            {
                var i = 6 * Nr;
                for (var j = 0; j < 6; j++)
                {
                    SMM[i + j, i + j] += ExtraNodalMass[j];
                }

                totalMass += ExtraNodalMass[0];
            }

            /// <summary>
            /// read node's extra mass.
            /// </summary>
            public void ReadNodeExtraMass(double[] mass)
            {
                ExtraNodalMass = new DenseVector(6);
                ExtraNodalMass[0] = ExtraNodalMass[1] = ExtraNodalMass[2] = mass[0];
                ExtraNodalMass[3] = mass[1];
                ExtraNodalMass[4] = mass[2];
                ExtraNodalMass[5] = mass[3];
            }
        }
    }
}