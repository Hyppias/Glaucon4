#region FileHeader
// Project: Glaucon4
// Filename:   Member.cs
// Last write: 4/30/2023 2:29:48 PM
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

//using static Math;

namespace Terwiel.Glaucon
{

    public partial class Glaucon
    {
        [Serializable]
        public partial class Member
        {
            #region Properties

            [XmlAttribute("NodeA"), JsonProperty(nameof(NodeA))]
            [Description("Begin node number A")]
            public Node NodeA { get; set; }

            [XmlAttribute("NodeB"), JsonProperty(nameof(NodeB))]
            [Description("End node number B")]
            public Node NodeB { get; set; }

            [XmlAttribute("MemberNr"), JsonProperty(nameof(Nr))]
            [Description("Member number")]
            public int Nr { get; set;} // member Nr base 0. not REALLY relevant.

            [XmlVector("Iz"), JsonProperty(nameof(Iz))]
            [Description("Three member inertias, perpendicular to the YZ plane")]
            public DenseVector Iz { get;set; }

            [XmlAttribute("EffLength"), JsonProperty(nameof(L_eff))]
            [Description("Effective length (due to node size)")]
            public double L_eff { get;set; }

            [XmlVector("As"), JsonProperty(nameof(As))]
            [Description("Cross section area components")]
            public DenseVector As { get; set;}

            [XmlAttribute("Length"), JsonProperty(nameof(Length))]
            [Description("Nominal member length")]
            public double Length { get;set; }

            [XmlAttribute("Roll"), JsonProperty(nameof(Roll))]
            [Description("Roll of each member, radians")]
            public double Roll { get; set;}

            [XmlAttribute("Material"), JsonProperty(nameof(Mat))]
            [Description("Material, the member is made of")]
            public Material Mat { get; set;}

            [XmlElement("Mass"), JsonProperty(nameof(Mass))]
            [Description("Member mass")]
            public double Mass { get; set;}

            [XmlElement("TotalMass"), JsonProperty(nameof(TotalMass))]
            [Description("Mass including extra mass")]
            public double TotalMass { get; set; }

            [XmlMatrix("Gamma", Packing.Sparse)]
            [Description("Transformation matrix")]
            public DenseMatrix Gamma { get; set;}

            #endregion Properties

            #region Fields
            
            public int XIncrementCount;
            public double
                axial_strain,
                Ksy,
                Ksz, // Shear deformation coefficients
                Dsy = 1,
                Dsz = 1;

            public double EMs; // Extra mass on member

            public int nA, nB;
            [XmlIgnore]
            [Description("Member stiffness matrix in global coord")]
            public DenseMatrix k;

            //public DenseVector Q;

            public int[] ind; // member-structure DoF index table. It loses significance after Permutation
            // public double IncrPeak;

            #endregion Fields

            #region constructors

            public Member(int nr, int ndA, int ndB, double[] as_, double[] iz ,double[] material, double roll, bool active = true)
            {
                Nr = nr;
                nA = ndA-1;
                nB = ndB-1;

                As = DenseVector.OfArray(as_);
                Iz = DenseVector.OfArray(iz);
                As = as_;
                Iz = iz;
                //As[0] = as_[0];
                //As[1] = as_[1];
                //As[2] = as_[2];
                //Iz[0] = iz[0];
                //Iz[1] = iz[1];
                //Iz[2] = iz[2];
                Mat = new Material(material[0],material[1],material[2],material[3]);
                //Mat.Alpha = material[3];
                //Mat.Density = material[2];
                //Mat.G = material[1];
                //Mat.E = material[0];
                Roll = roll;
                Active = true; // members may never be made inactive.
                
            }

            public bool Active;
            /// <summary>
            /// Constructor for Member
            /// </summary>
            /// <param name="n">member number base 0</param>
            /// <param name="nodes">the nodes list</param>
            public void Process(List<Node> nodes) // n is base 0
            {
#if DEBUG
                Prefix = $"mb_{Nr + 1}_";
#endif
                //Nr = n;
                k = new DenseMatrix(12); //  may include the geometric part
                Gamma = new DenseMatrix(12);
                ind = new int[12];                

                NodeA.Active = NodeB.Active = true; // to check if all nodes are relevant

                // each member knows where it goes in the system stiffness matrix:
                var n1 = NodeA.Nr * 6;
                var n2 = NodeB.Nr * 6;
                for (var i = 0; i < 6; i++)
                {
                    ind[i] = n1 + i;
                    ind[i + 6] = n2 + i;
                }

                Length = Math.Sqrt(
                    Sq(NodeB.Coord[0] - NodeA.Coord[0]) +
                    Sq(NodeB.Coord[1] - NodeA.Coord[1]) +
                    Sq(NodeB.Coord[2] - NodeA.Coord[2])
                );
                // for ForceBentBeam() (original: nx)
                XIncrementCount = (int) Math.Floor(Length / Param.XIncrement); 
                // Effective length: minus the node radius:
                L_eff = Length - NodeA.NodeRadius - NodeB.NodeRadius;
                if (L_eff < 0)
                {
                    throw new ArgumentOutOfRangeException($"Node radii too large for member {Nr + 1}.");
                }

                SetupGamma(NodeA.Coord, NodeB.Coord);

                TotalMass = Mass = Mat.Density * As[0] * Length;
            }

            #endregion

            // See Przemieniecki pg 80.
            // this method is only executed if shear is true!
            public void SetupShearDeformationFactors(bool shear = true)
            {
                if (shear)
                {
                    Ksy = 12.0 * Mat.E * Iz[2] / (Mat.G * As[1] * L_eff * L_eff); // Φy: Przemieniecki formula 5.117
                    Ksz = 12.0 * Mat.E * Iz[1] / (Mat.G * As[2] * L_eff * L_eff); // Φz: Przemieniecki formula 5.118
                    Dsy = Sq(1.0d + Ksy);
                    Dsz = Sq(1.0d + Ksz);
                }
                else
                {
                    // this is never executed.
                    // neglect Shear force deformations:
                    Ksy = Ksz = 0.0;
                    Dsy = Dsz = 1.0;
                }
            }

            /// <summary>
            /// space frame elastic stiffness matrix in global coordinates
            /// </summary>
            public void Elastic_K()
            {
                k.Clear();
                // first the local stiffness matrix:
                k[6, 0] = k[0, 6] = -(k[0, 0] = k[6, 6] = Mat.E * As[0] / L_eff);
                //k[1, 1] = k[7, 7] = 12.0 * E * Iz[2] / (L_eff * L_eff * L_eff * (1.0 + Ksy));
                //k[2, 2] = k[8, 8] = 12.0 * E * Iz[1] / (L_eff * L_eff * L_eff * (1.0 + Ksz));
                //k[3, 3] = k[9, 9] = G * Iz[0] / L_eff;
                k[4, 4] = k[10, 10] = (4.0 + Ksz) * Mat.E * Iz[1] / (L_eff * (1.0 + Ksz));
                k[5, 5] = k[11, 11] = (4.0 + Ksy) * Mat.E * Iz[2] / (L_eff * (1.0 + Ksy));

                // k[4, 2] = k[2, 4] = -6.0 * E * Iz[1] / (L_eff * L_eff * (1.0 + Ksz));
                // k[5, 1] = k[1, 5] =  6.0 * E * Iz[2] / (L_eff * L_eff * (1.0 + Ksy));
                //k[6, 0] = k[0, 6] = -k[0, 0];

                k[11, 7] = k[7, 11] = k[7, 5] = k[5, 7] = -(
                    k[11, 1] = k[1, 11] = k[5, 1] = k[1, 5] = 6.0 * Mat.E * Iz[2] / (L_eff * L_eff * (1.0 + Ksy))
                );
                k[10, 8] = k[8, 10] = k[8, 4] = k[4, 8] = -(
                    k[10, 2] = k[2, 10] = k[4, 2] = k[2, 4] = -6.0 * Mat.E * Iz[1] / (L_eff * L_eff * (1.0 + Ksz))
                );
                k[9, 3] = k[3, 9] = -(k[3, 3] = k[9, 9] = Mat.G * Iz[0] / L_eff);
                //k[10, 2] = k[2, 10] = k[4, 2];
                // k[11, 1] = k[1, 11] = k[5, 1];

                k[7, 1] = k[1, 7] = -(k[1, 1] = k[7, 7] = 12.0 * Mat.E * Iz[2] / (L_eff * L_eff * L_eff * (1.0 + Ksy)));
                k[8, 2] = k[2, 8] = -(k[2, 2] = k[8, 8] = 12.0 * Mat.E * Iz[1] / (L_eff * L_eff * L_eff * (1.0 + Ksz)));
                k[10, 4] = k[4, 10] = (2.0 - Ksz) * Mat.E * Iz[1] / (L_eff * (1.0 + Ksz));
                k[11, 5] = k[5, 11] = (2.0 - Ksy) * Mat.E * Iz[2] / (L_eff * (1.0 + Ksy));

                // See McGuire, pg 98, formula 5.16
                k = (DenseMatrix)Gamma.TransposeThisAndMultiply(k) * Gamma; // globalize
            }

            /// <summary>
            /// Get the load vector due to gravity on the elements
            /// </summary>
            /// <param name="g_">Vector of Accelleration of gravity</param>
            /// <returns>Gravity load vector (global)</returns>
            public DenseVector GetGravityLoadVector(DenseVector g_)
            {
                var g = new DenseVector(12);

                var temp = g_ / 2f;
                g.SetSubVector(0, 3, temp);
                g.SetSubVector(6, 3, temp);
                temp[0] = Length / 6d * // was already divided by 2 (g_ !)
                    ((-Gamma[1, 0] * Gamma[2, 1] + Gamma[1, 1] * Gamma[2, 0]) * g[1] +
                        (-Gamma[1, 0] * Gamma[2, 2] + Gamma[1, 2] * Gamma[2, 0]) * g[2]);
                temp[1] = Length / 6d *
                    ((-Gamma[1, 1] * Gamma[2, 0] + Gamma[1, 0] * Gamma[2, 1]) * g[0] +
                        (-Gamma[1, 1] * Gamma[2, 2] + Gamma[1, 2] * Gamma[2, 1]) * g[2]);
                temp[2] = Length / 6d *
                    ((-Gamma[1, 2] * Gamma[2, 0] + Gamma[1, 0] * Gamma[2, 2]) * g[0] +
                        (-Gamma[1, 2] * Gamma[2, 1] + Gamma[1, 1] * Gamma[2, 2]) * g[1]);

                g.SetSubVector(3, 3, temp);
                g.SetSubVector(9, 3, -temp);
                g *= (Mat.Density * As[0] * Length); // As[0] is area cross section
                return g;
            }

            /// <summary>
            /// space frame geometric stiffness matrix, global coordinates.
            /// This part of the stiffness matrix is not stored separately.
            /// It is immediately added to the elastic stiffness matrix.
            /// See Przemienniecki pg 384
            /// </summary>
            /// <param name="T">Axial force due to temperature change</param>
            public void GeometricMemberStiffnessMatrix(double T)
            {
                var kg = new DenseMatrix(12);

                // See McGuire pg 257, formula 9.18
                // See Przemieniecki, pg 401
                kg[0, 0] = kg[6, 6] = 0.0; // T/Length;

                kg[1, 1] = kg[7, 7] = T / Length * (1.2 + 2.0 * Ksy + Ksy * Ksy) / Dsy;
                kg[2, 2] = kg[8, 8] = T / Length * (1.2 + 2.0 * Ksz + Ksz * Ksz) / Dsz;
                kg[3, 3] = kg[9, 9] = T / Length * Iz[0] / As[0];
                kg[4, 4] = kg[10, 10] = T * Length * (2.0 / 15.0 + Ksz / 6.0 + Ksz * Ksz / 12.0) / Dsz;
                kg[5, 5] = kg[11, 11] = T * Length * (2.0 / 15.0 + Ksy / 6.0 + Ksy * Ksy / 12.0) / Dsy;

                kg[0, 6] = kg[6, 0] = 0.0; // -T/Length;

                kg[4, 2] = kg[2, 4] = kg[10, 2] = kg[2, 10] = -T / 10.0 / Dsz;
                kg[8, 4] = kg[4, 8] = kg[10, 8] = kg[8, 10] = T / 10.0 / Dsz;
                kg[5, 1] = kg[1, 5] = kg[11, 1] = kg[1, 11] = T / 10.0 / Dsy;
                kg[7, 5] = kg[5, 7] = kg[11, 7] = kg[7, 11] = -T / 10.0 / Dsy;

                kg[3, 9] = kg[9, 3] = -kg[3, 3];

                kg[7, 1] = kg[1, 7] = -kg[7, 7]; // -T / Length * (1.2 + 2.0 * Ksy + Ksy * Ksy) / Dsy;
                kg[8, 2] = kg[2, 8] = -kg[8, 8]; // -T / Length * (1.2 + 2.0 * Ksz + Ksz * Ksz) / Dsz;

                kg[10, 4] = kg[4, 10] = -T * Length * (1.0 / 30.0 + Ksz / 6.0 + Ksz * Ksz / 12.0) / Dsz;
                kg[11, 5] = kg[5, 11] = -T * Length * (1.0 / 30.0 + Ksy / 6.0 + Ksy * Ksy / 12.0) / Dsy;
                // See McGuire pg 98, formula 5.16
                kg = (DenseMatrix)Gamma.TransposeThisAndMultiply(kg) * Gamma; // globalize 

                // check and enforce symmetry of geometric member stiffness matrix 

                //ForceSymmetry(kg, 1.0e-6, "Geom. stiffness matrix kg");

                // add global member geometric stiffness matrix to global member elastic stiffness matrix . 
                k += kg;
                //WriteMatrix(MEMBER,  "E", "KplusGeom.dat", "kplusgeom", k);
            }
#if false
        private void ForceSymmetry(DenseMatrix m, double err, string name)
        {
            for (int i = 0; i < m.ColumnCount; i++)
                for (int j = i + 1; j < m.RowCount; j++)
                    if (m[i, j] != m[j, i])
                    {
                        if (Abs(m[i, j] / m[j, i] - 1.0) > err
                            && (Abs(m[i, j] / m[i, i]) > err
                                || Abs(m[j, i] / m[i, i]) > err
                            )
                        )
                        {
                            Errors.Add($"Member {Nr}: {name} is not symmetrical." + Environment.NewLine +
                                  $" . m[{i},{j}] = {m[i, j]}" + Environment.NewLine +
                                  $" . m[{j},{i}] = {m[j, i]}" + Environment.NewLine +
                                  $" . relative error = {Abs(m[i, j] / m[j, i] - 1.0)} ");
                        }
                        m[i, j] = m[j, i] = 0.5 * (m[i, j] + m[j, i]);
                    }
        }
#endif
            /// <summary>
            /// Evaluate the end forces in local coord
            /// Original name: element_end_forces
            /// </summary>
            /// <param name="d">global displacement vector for two nodes</param>
            /// <param name="equivForces">global fixed-end-forces</param>
            /// <returns>Member end force vector</returns>
            public DenseVector MemberEndForces(DenseVector d, DenseVector equivForces)
            {
                var s = new DenseVector(12);
                // See McGuire, Gallagher & Ziemian, Matrix Structural Analysis, 2nd ed., pg 98/9:
                // [F'] = [Γ] * [k] * [Δ] = local force vector

                // translations:
                axial_strain = (d.SubVector(6, 3) - d.SubVector(0, 3)) * Gamma.Row(0).SubVector(0, 3) / L_eff;
                if (Math.Abs(axial_strain) > Param.StrainLimit)
                {
                    Errors.Add($"Member {Nr + 1} has excess axial strain.");
                    Param.AxialStrainWarning++; // this member has axial strain
                }
                // [k] is the GLOBAL member stiffness matrix
                // [d] is GLOBAL member displacement vector.
                // The GLOBAL member end force vector is:
                // [Q] = [k] * [d] + [eqF],
                // where eqF is the sum of the equivalent fixed member end mechanical force vector and the
                // equivalent fixed member end temperature force vector
                // to localize Q:
                // [Q']= [Γ] * [Q]

#if true

                // double t1, t2, t3, t4, t5, t6, t7, t8, t9;
                var t1 = Gamma[0, 0];
                var t2 = Gamma[0, 1];
                var t3 = Gamma[0, 2];
                var t4 = Gamma[1, 0];
                var t5 = Gamma[1, 1];
                var t6 = Gamma[1, 2];
                var t7 = Gamma[2, 0];
                var t8 = Gamma[2, 1];
                var t9 = Gamma[2, 2];
                s[0] = -(As[0] * Mat.E / L_eff) * ((d[6] - d[0]) * t1 + (d[7] - d[1]) * t2 + (d[8] - d[2]) * t3);

                var T = Param.AccountForGeomStability ? -s[0] : 0;

                double Le3, Le2;
                s[1] = -(12.0 * Mat.E * Iz[2] / ((Le3 = L_eff * (Le2 = L_eff * L_eff)) * (1.0 + Ksy))
                        + T / Length * (1.2 + 2.0 * Ksy + Ksy * Ksy) / Dsy) *
                    ((d[6] - d[0]) * t4 + (d[7] - d[1]) * t5 + (d[8] - d[2]) * t6)
                    + (6.0 * Mat.E * Iz[2] / (Le2 * (1.0 + Ksy)) + T / 10.0 / Dsy) *
                    ((d[3] + d[9]) * t7 + (d[4] + d[10]) * t8 + (d[5] + d[11]) * t9);

                s[2] = -(12.0 * Mat.E * Iz[1] / (Le3 * (1.0 + Ksz))
                        + T / Length * (1.2 + 2.0 * Ksz + Ksz * Ksz) / Dsz) *
                    ((d[6] - d[0]) * t7 + (d[7] - d[1]) * t8 + (d[8] - d[2]) * t9)
                    - (6.0 * Mat.E * Iz[1] / (Le2 * (1.0 + Ksz)) + T / 10.0 / Dsz) *
                    ((d[3] + d[9]) * t4 + (d[4] + d[10]) * t5 + (d[5] + d[11]) * t6);

                s[3] = -(Mat.G * Iz[0] / L_eff) * ((d[9] - d[3]) * t1 + (d[10] - d[4]) * t2 + (d[11] - d[5]) * t3);

                s[4] = (6.0 * Mat.E * Iz[1] / (Le2 * (1.0 + Ksz)) + T / 10.0 / Dsz) *
                    ((d[6] - d[0]) * t7 + (d[7] - d[1]) * t8 + (d[8] - d[2]) * t9)
                    + ((4.0 + Ksz) * Mat.E * Iz[1] / (L_eff * (1.0 + Ksz)) +
                        T * Length * (2.0 / 15.0 + Ksz / 6.0 + Ksz * Ksz / 12.0) / Dsz) *
                    (d[3] * t4 + d[4] * t5 + d[5] * t6)
                    + ((2.0 - Ksz) * Mat.E * Iz[1] / (L_eff * (1.0 + Ksz)) -
                        T * Length * (1.0 / 30.0 + Ksz / 6.0 + Ksz * Ksz / 12.0) / Dsz) *
                    (d[9] * t4 + d[10] * t5 + d[11] * t6);
                s[5] = -(6.0 * Mat.E * Iz[2] / (Le2 * (1.0 + Ksy)) + T / 10.0 / Dsy) *
                    ((d[6] - d[0]) * t4 + (d[7] - d[1]) * t5 + (d[8] - d[2]) * t6)
                    + ((4.0 + Ksy) * Mat.E * Iz[2] / (L_eff * (1.0 + Ksy)) +
                        T * Length * (2.0 / 15.0 + Ksy / 6.0 + Ksy * Ksy / 12.0) / Dsy) *
                    (d[3] * t7 + d[4] * t8 + d[5] * t9)
                    + ((2.0 - Ksy) * Mat.E * Iz[2] / (L_eff * (1.0 + Ksy)) -
                        T * Length * (1.0 / 30.0 + Ksy / 6.0 + Ksy * Ksy / 12.0) / Dsy) *
                    (d[9] * t7 + d[10] * t8 + d[11] * t9);
                s[6] = -s[0];
                s[7] = -s[1];
                s[8] = -s[2];
                s[9] = -s[3];

                s[10] = (6.0 * Mat.E * Iz[1] / (Le2 * (1.0 + Ksz)) + T / 10.0 / Dsz) *
                    ((d[6] - d[0]) * t7 + (d[7] - d[1]) * t8 + (d[8] - d[2]) * t9)
                    + ((4.0 + Ksz) * Mat.E * Iz[1] / (L_eff * (1.0 + Ksz)) +
                        T * Length * (2.0 / 15.0 + Ksz / 6.0 + Ksz * Ksz / 12.0) / Dsz) *
                    (d[9] * t4 + d[10] * t5 + d[11] * t6)
                    + ((2.0 - Ksz) * Mat.E * Iz[1] / (L_eff * (1.0 + Ksz)) -
                        T * Length * (1.0 / 30.0 + Ksz / 6.0 + Ksz * Ksz / 12.0) / Dsz) *
                    (d[3] * t4 + d[4] * t5 + d[5] * t6);

                s[11] = -(6.0 * Mat.E * Iz[2] / (Le2 * (1.0 + Ksy)) + T / 10.0 / Dsy) *
                    ((d[6] - d[0]) * t4 + (d[7] - d[1]) * t5 + (d[8] - d[2]) * t6)
                    + ((4.0 + Ksy) * Mat.E * Iz[2] / (L_eff * (1.0 + Ksy)) +
                        T * Length * (2.0 / 15.0 + Ksy / 6.0 + Ksy * Ksy / 12.0) / Dsy) *
                    (d[9] * t7 + d[10] * t8 + d[11] * t9)
                    + ((2.0 - Ksy) * Mat.E * Iz[2] / (L_eff * (1.0 + Ksy)) -
                        T * Length * (1.0 / 30.0 + Ksy / 6.0 + Ksy * Ksy / 12.0) / Dsy) *
                    (d[3] * t7 + d[4] * t8 + d[5] * t9);
#else
            Elastic_K();
            // double T;
            // double t1, t2, t3, t4, t5, t6, t7, t8, t9;
            // double t1 = Gamma[0, 0]; double t2 = Gamma[0, 1]; double t3 = Gamma[0, 2];
            //double t4 = Gamma[1, 0]; double t5 = Gamma[1, 1]; double t6 = Gamma[1, 2];
            //double t7 = Gamma[2, 0]; double t8 = Gamma[2, 1]; double t9 = Gamma[2, 2];
            //double A = -((double) As[0] * (double)E / L_eff) * (double) ((d[6] - d[0]) * t1 + (d[7] - d[1]) * t2 + (d[8] - d[2]) * t3);
           //  double A = (d.SubVector(6,3) - d.SubVector(0,3)) * Gamma.Row(0);
            //T = Geom ? A : 0;

            if (Geom)
                GeometricMemberStiffnessMatrix((d.SubVector(6,3) - d.SubVector(0,3)) * Gamma.SubMatrix(0,3,0,3).Row(0));
            Q = Gamma * k * d; // and localize
#endif
                // transform equiv. forces to local member coordinate system and
                // add local fixed end forces - equivalent loads to internal loads 
                // {Q'} = [Γ] * [F]
                return s - Gamma * equivForces; // Q = local member end forces
            }

            /// <summary>
            /// LumpedMassMatrix
            /// (does not include Shear deformations)
            /// </summary>
            /// <returns>Lumped mass matrix in global coordinates for this member</returns>
            public DenseMatrix LumpedMassMatrix()
            {
                var m = new DenseMatrix(12);
                var t_ = (Mat.Density * As[0] * Length + EMs) / 2.0;
                var ry = Mat.Density * Iz[1] * Length / 2.0;
                var rz = Mat.Density * Iz[2] * Length / 2.0;
                var po = Mat.Density * Length * Iz[0] / 2.0; // assumes simple cross-section	

                m[1, 1] = m[2, 2] = m[0, 0] = m[7, 7] = m[8, 8] = m[6, 6] = t_;
                var g = Gamma.PointwiseMultiply(Gamma);
                m[3, 3] = m[9, 9] = po * g[0, 0] + ry * g[1, 0] + rz * g[2, 0];
                m[4, 4] = m[10, 10] = po * g[0, 1] + ry * g[1, 1] + rz * g[2, 1];
                m[5, 5] = m[11, 11] = po * g[0, 2] + ry * g[1, 2] + rz * g[2, 2];

                m[3, 4] = m[4, 3] = m[9, 10] = m[10, 9] = po * Gamma[0, 0] * Gamma[0, 1] +
                    ry * Gamma[1, 0] * Gamma[1, 1] + rz * Gamma[2, 0] * Gamma[2, 1];
                m[3, 5] = m[5, 3] = m[9, 11] = m[11, 9] = po * Gamma[0, 0] * Gamma[0, 2] +
                    ry * Gamma[1, 0] * Gamma[1, 2] + rz * Gamma[2, 0] * Gamma[2, 2];
                m[4, 5] = m[5, 4] = m[10, 11] = m[11, 10] = po * Gamma[0, 1] * Gamma[0, 2] +
                    ry * Gamma[1, 1] * Gamma[1, 2] + rz * Gamma[2, 1] * Gamma[2, 2];

                return m;
            }

            /// <summary>
            /// ConsistentMassMatrix
            /// (does not include Shear deformations)
            /// </summary>
            /// <returns>Consistent mass matrix in global coordinates for this member</returns>
            public DenseMatrix ConsistentMassMatrix()
            {
                // translational, rotational & polar inertia 
                var t_ = Mat.Density * As[0] * Length;
                double ry = Mat.Density * Iz[1];
                double rz = Mat.Density * Iz[2];
                var po = Mat.Density * Iz[0] * Length;

                var m = new DenseMatrix(12);

                m[0, 0] = m[6, 6] = t_ / 3.0;
                m[1, 1] = m[7, 7] = 13.0 * t_ / 35.0 + 6.0 * rz / (5.0 * Length);
                m[2, 2] = m[8, 8] = 13.0 * t_ / 35.0 + 6.0 * ry / (5.0 * Length);
                m[3, 3] = m[9, 9] = po / 3.0;
                var temp = t_ * Length * Length / 105.0;
                m[4, 4] = m[10, 10] = temp + 2.0 * Length * ry / 15.0;
                m[5, 5] = m[11, 11] = temp + 2.0 * Length * rz / 15.0;
                var temp2 = 11.0 * t_ * Length / 210.0;
                m[4, 2] = m[2, 4] = -temp2 - ry / 10.0;
                m[5, 1] = m[1, 5] = temp2 + rz / 10.0;
                m[6, 0] = m[0, 6] = t_ / 6.0;

                temp = 13.0 * t_ * Length / 420.0;
                m[7, 5] = m[5, 7] = temp - rz / 10.0;
                m[8, 4] = m[4, 8] = -temp + ry / 10.0;
                m[9, 3] = m[3, 9] = po / 6.0;
                m[10, 2] = m[2, 10] = temp - ry / 10.0;
                m[11, 1] = m[1, 11] = -temp + rz / 10.0;

                m[10, 8] = m[8, 10] = temp2 + ry / 10.0;
                m[11, 7] = m[7, 11] = -temp2 - rz / 10.0;

                m[7, 1] = m[1, 7] = 9.0 * t_ / 70.0 - 6.0 * rz / (5.0 * Length);
                m[8, 2] = m[2, 8] = 9.0 * t_ / 70.0 - 6.0 * ry / (5.0 * Length);
                m[10, 4] = m[4, 10] = (temp = -Length * Length * t_ / 140.0) - ry * Length / 30.0;
                m[11, 5] = m[5, 11] = temp - rz * Length / 30.0;

                // rotatory inertia of extra beam mass is neglected 

                for (var i = 0; i < 3; i++)
                {
                    m[i, i] += 0.5 * EMs;
                    m[i + 6, i + 6] += 0.5 * EMs;
                }

                //WriteMatrix(true, $"Mo1-mbr{Nr}_", "Consis", "ConsisentMM", m);
                m = (DenseMatrix)Gamma.TransposeThisAndMultiply(m) * Gamma; // globalize.          

                // WriteMatrix(true, $"Mo2-mbr{Nr}_", "Consis", "ConsisentMM", m);
                return m;
            }

            public void AddLocalToGlobal(DenseMatrix global, DenseMatrix member)
            {
                for (var j = 0; j < 12; j++)
                {
                    var ii = ind[j];
                    for (var _k = 0; _k < 12; _k++)
                    {
                        global[ii, ind[_k]] += member[j, _k];
                    }
                }
            }
        }
    }
}
