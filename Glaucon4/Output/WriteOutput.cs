#region FileHeader
// Project: Glaucon4
// Filename:   WriteOutput.cs
// Last write: 4/30/2023 2:29:51 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

namespace Terwiel.Glaucon
{

    /// <summary>
    /// This part of the class Glaucon has the routines to
    /// print the calculated data to an HTML/CSV/Excel file
    /// </summary>
    public partial class Glaucon
    {
        private int row, tableNr;

        public IEnumerable<object> PrescrDisplacementList { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="WriteString">Output a string</param>
        /// <param name="TableNoHeader">Output a two-column table of data</param>
        /// <param name="TableRow">Output a table row</param>
        /// <param name="BlankLine">output an empty line</param>
        /// <param name="Table">Output the entire table with header</param>
        /// <param name="format">Format to use for doubles</param>
        private void WriteOutput(Action<string, string> WriteString, Action<List<object[]>> TableNoHeader,
            Action<object[]> TableRow, Action BlankLine,
            Action<string[][], int[], Action> Table, Func<double, string> format)
        {
            string ws(int n)
            {
                return n != 1 ? "s" : string.Empty;
            }

            row = 1;
            string[] cols;
            tableNr = 1;
            try
            {
                WriteString("h1", $"{ProgramName} v. {ProgramVersion}");
                WriteString("", "A program to compute 2D and 3D trusses and frames");
                WriteString("", "By E.H. Terwiel, 2020, based on:");
                WriteString("", "Frame3DD: GPL Copyright (C) 1992-2015, by Henri P. Gavin, http://frame3dd.sf.net/ ");
                //WriteString($"Uses {Control.LinearAlgebraProvider.ToString()}");
                WriteString("", $"Date/time: {DateTime.Now.ToString(culture.DateTimeFormat.FullDateTimePattern)}");
                WriteString("", Title);

                WriteString("", $"The {(Dim3 ? "Z" : "Y")}-axis is vertical.");

                BlankLine();

                WriteString("large", "Input data");

                var t_ =
                    new List<object[]>
                    {
                    new object[] {"nodes", Nodes.Count},
                    new object[] {$"fixed node{ws(NodeRestraints.Count)}", NodeRestraints.Count},
                    new object[] {$"Member{ws(Members.Count)}", Members.Count},
                    new object[] {$"load case{ws(LoadCases.Count)}", LoadCases.Count},
                    new object[] {"Structural mass", StructMass}
                    };

                if (Members.Count > 0)
                {
                    t_.Add(new object[] { "Total mass", TotalMass });
                    t_.Add(new object[] { "Mass matrix", Param.LumpedMassMatrix ? "Lumped" : "Consistent" });
                    t_.Add(new object[] { "Convergence tolerance", Param.ModalConvergenceTol });
                }

                t_.Add(new object[] { $"Shear deformation {(Param.AccountForShear ? "" : "not")} included" });
                t_.Add(new object[] { $"Geometric stiffness calculation {(Param.AccountForGeomStability ? "" : "not")} included" });

                TableNoHeader(t_);

                Table(new[]
                    {
                        new[] {"Node data"},
                        new[] {"Node", "Coordinates", "", "Restraints ¹)"},
                        cols = new[] {"", "X", "Y", "Z", "Radius", "X", "Y", "Z", "X", "Y", "Z"}
                        // this array has the widths of the columns for the second string array.
                        // so both arrays must be of equal length.
                    }, 
                    new[] { 1, 3, 1, 6 }, () =>
                        {
                            var temp1 = new object[11];
                            foreach (var nd in Nodes)
                            {
                                var i = 0;
                                temp1[i++] = nd.Nr + 1;
                                for (var j = 0; j < 3; j++)
                                {
                                    temp1[i++] = nd.Coord[j];
                                }

                                temp1[i++] = nd.NodeRadius;
                                // restrained DoFs are marked with a cross:
                                for (var j = 0; j < 6; j++)
                                {
                                    temp1[i++] = nd.Restraints[j] == 1 ? "x" : " ";
                                }

                                TableRow(temp1);
                            }
                        }
                );

                WriteString("small", "¹) x = restrained in this sense or direction");

                BlankLine();

                Table(new[]
                    {
                        new[] {"Frame members data (local)"},
                        new[] {"Mbr", "Nodes", "Cross section", "Material"},
                        new[] {"", "A", "B", "Ax", "Asy", "Asz", "Jxx", "Iyy", "Izz", "roll", "E", "G", "Density"}
                    }, new[] { 1, 2, 7, 3 }, () =>
                    {
                        foreach (var mbr in Members)
                        {
                            TableRow(new object[]
                            {
                                mbr.Nr + 1, mbr.NodeA.Nr + 1, mbr.NodeB.Nr + 1,
                                mbr.As[0], mbr.As[1], mbr.As[2],
                                mbr.Iz[0], mbr.Iz[1], mbr.Iz[2],
                                mbr.Roll, mbr.Mat.E, mbr.Mat.G, (double) mbr.Mat.Density
                            });
                        }
                    }
                );

                WriteString("",
                    "Density in tonne/cubic mm: so input 7800 kilogram/cubic meter as 7.8e-9 tonne/cubic mm");
                foreach (var ld in LoadCases)
                {
                    BlankLine();
                    WriteString("large", $"Load case {ld.Nr + 1}");

                    t_.Clear();

                    for (var i = 0; i < 3; i++)
                    {
                        t_.Add(new object[] { $"Gravity {"XYZ"[i]}", ld.g[i] });
                    }

                    t_.Add(new object[] { "concentrated loads.", ld.NodalLoads.Count });
                    t_.Add(new object[] { $"uniformly distributed load{ws(ld.UniformLoads.Count)}", ld.UniformLoads.Count });
                    t_.Add(new object[] { $"trapezoidally distributed load{ws(ld.UniformLoads.Count)}", ld.UniformLoads.Count });
                    t_.Add(new object[] { $"concentrated point load{ws(ld.IntPointLoads.Count)}", ld.IntPointLoads.Count });
                    t_.Add(new object[] { $"temperature load{ws(ld.TempLoads.Count)}", ld.TempLoads.Count });
                    t_.Add(new object[] { $"prescribed displacement{ws(ld.PrescrDisplacements.Count)}", ld.PrescrDisplacements.Count });

                    var lg = new[] { 1e-24, 1e-16, 1e-12, double.MaxValue };
                    var quality = new[] { "excellent", "good", "adequate", "not adequate" };
                    int q;
                    for (q = 0; q < lg.Length; q++)
                    {
                        if (ld.RMS_Error < lg[q])
                        {
                            break;
                        }
                    }

                    t_.Add(new object[] { "RMS Relative equilibrium error", ld.RMS_Error });
                    if (Param.AccountForGeomStability)
                    {
                        t_.Add(new object[] { "Iterations to reach equilibrium", Param.Iterations });
                    }

                    TableNoHeader(t_);

                    Table(new[]
                        {
                            new[] {"Nodal loads + equivalent nodal loads (global)"},
                            new[] {"Node", "Forces", "Moments"},
                            new[] {"", "X", "Y", "Z", "X", "Y", "Z"}
                        }, new[] { 1, 3, 3 }, () =>
                        {
                            var temp = new object[7];
                            for (var j = 0; j < ld.NodalLoads.Count; j++)
                            {
                                var ndNr = ld.NodalLoads[j].NodeNr;
                                temp[0] = ndNr + 1;
                                for (var i = 0; i < 6; i++)
                                {
                                    temp[i + 1] = ld.FMech[ndNr * 6 + i, 0];
                                }

                                TableRow(temp);
                            }
                        }
                    );
                    if (ld.UniformLoads.Count > 0) //Uniformly distributed loads
                    {
                        Table(new[]
                            {
                                new[] {"Uniformly distributed loads (local)"},
                                new[] {"Node", "Loads"},
                                new[] {"", "X", "Y", "Z"}
                            }, new[] { 1, 3 }, () =>
                            {
                                var temp = new object[4];
                                foreach (var unif in ld.UniformLoads)
                                {
                                    temp[0] = unif.MemberNr + 1;
                                    for (var i = 0; i < 3; i++)
                                    {
                                        temp[i + 1] = unif.Q[i];
                                    }

                                    TableRow(temp);
                                }
                            }
                         );
                    } // end Uniform loads

                    if (ld.TrapLoads.Count > 0) //Trapezoidally distributed loads
                    {
                        BlankLine();
                        Table(new[]
                            {
                                new[] {"Trapezoidally distributed loads (local)"},
                                new[] {"Mbr", "Position", "Load at"},
                                new[] {"", "start", "end", "start", "end"}
                            }, new[] { 1, 2, 2 }, () =>
                            {
                                foreach (var tl in ld.TrapLoads)
                                    foreach (var lds in tl.Loads)
                                    {
                                        TableRow(new object[] { tl.MemberNr + 1, lds.a, lds.b, lds.Wa, lds.Wb });
                                    }
                            }
                        );
                    } // end TrapLoads

                    if (ld.TempLoads.Count > 0) // Temperature changes
                    {
                        Table(new[]
                            {
                                new[] {"Temperature changes (local)"},
                                new[] {"Mbr", "α", "Depth", "Δ T"},
                                new[] {"", "", "y", "z", "y+", "y-", "z+", "z-"}
                            }, new[] { 1, 1, 2, 4 }, () =>
                            {
                                foreach (var lds in ld.TempLoads)
                                {
                                    TableRow(new object[]
                                        {lds.MemberNr + 1, lds.alpha, lds.hy, lds.hz, lds.typ, lds.tym, lds.tzp, lds.tzm});
                                }
                            }
                        );
                    }

                    if (ld.IntPointLoads.Count > 0) //Concentrated point loads
                    {
                        Table(new[]
                            {
                                new[] {"Concentrated point loads (local)"},
                                new[] {"Mbr", "", "Components"},
                                new[] {"", "Pos", "X", "Y", "Z"}
                            }, new[] { 1, 1, 3 }, () =>
                            {
                                var temp = new object[5];
                                foreach (var lds in ld.IntPointLoads)
                                {
                                    var k = 0;
                                    temp[k++] = lds.MemberNr + 1;
                                    temp[k++] = lds.Position;
                                    for (var i = 0; i < 3; i++)
                                    {
                                        temp[k++] = lds.Load[i];
                                    }

                                    TableRow(temp);
                                }
                            }
                        );
                    }

                    if (ld.PrescrDisplacements.Count > 0) // Prescribed displacements
                    {
                        Table(new[]
                            {
                                new[] {"Prescribed displacements (global)"},
                                new[] {"Mbr", "Translations", "Rotations"},
                                new[] {"", "X", "Y", "Z", "X", "Y", "Z"}
                            }, new[] { 1, 3, 3 }, () =>
                            {
                                var temp = new object[7];
                                foreach (var d in ld.PrescrDisplacements)
                                {
                                    temp[0] = d.NodeNr + 1;
                                    for (var k = 0; k < 6; k++)
                                    {
                                        temp[k + 1] = d.Displacements[k];
                                    }

                                    TableRow(temp);
                                }
                            }
                        );
                    }
                }

                BlankLine();
                WriteString("Large", "Output data");
                foreach (var ld in LoadCases)
                {
                    BlankLine();
                    WriteString("Large", $"Load case {ld.Nr + 1}");
                    Table(new[]
                        {
                            new[] {"Node displacements (global)"},
                            new[] {"Mbr", "Translations", "Rotations"},
                            new[] {"", "X", "Y", "Z", "X", "Y", "Z"}
                        }, new[] { 1, 3, 3 }, () =>
                        {
                            var temp = new object[7];
                            for (var j = 0; j < Nodes.Count; j++)
                                for (var i = 0; i < 6; i++)
                                {
                                    if (Nodes[j].Restraints[j * 6 + i] == 0)
                                    {
                                        var k = 0;
                                        temp[k++] = Nodes[j].Nr + 1;
                                        for (var m = 0; m < 6; m++)
                                        {
                                            temp[k++] = ld.Displacements[j * 6 + m, 0];
                                        }

                                        TableRow(temp);
                                        break;
                                    }
                                }
                        }
                        );

                    Table(new[]
                        {
                            new[] {"Frame element end forces (local)"},
                            new[] {"Mbr", "", "Forces", "Moments"},
                            new[] {"", "Node", "X", "²)", "Y", "Z", "X", "Y", "Z"}
                        }, new[] { 1, 1, 4, 3 }, () =>
                        {
                            var temp = new object[9];
                            var t = " ";
                            foreach (var mbr in Members)
                            {
                                // tensile/compressive force? 
                                var fA = ld.Q.Row(mbr.Nr)[0];
                                var fB = ld.Q.Row(mbr.Nr)[6];
                                if (fA != fB)
                                {
                                    t = fA > 0 && fB < 0 ? "c" : "t";
                                }

                                for (var j = 0; j < 12; j += 6)
                                {
                                    var k = 0;
                                    temp[k++] = mbr.Nr + 1;
                                    temp[k++] = (j == 0 ? mbr.NodeA.Nr : mbr.NodeB.Nr) + 1;
                                    for (var i = 0; i < 6; i++)
                                    {
                                        temp[k++] = ld.Q.Row(mbr.Nr)[j + i];
                                        if (i == 0)
                                        {
                                            temp[k++] = t;
                                        }
                                    }

                                    TableRow(temp);
                                }
                            }
                        }
                    );
                    WriteString("", "²) : t = member under tensile stress, c = compressed member");

                    Table(new[]
                        {
                            new[] {"Reactions (global)"},
                            new[] {"Node", "Forces", "Moments"},
                            new[] {"", "X", "Y", "Z", "X", "Y", "Z"}
                        }, new[] { 1, 3, 3 }, () =>
                        {
                            var temp = new object[7];
                            var k = MatrixPart;
                            foreach (var nd in Nodes)
                            {
                                for (var i = 0; i < 6; i++)
                                {
                                    if (nd.Restraints[i] == 1)
                                    {
                                        temp[0] = nd.Nr + 1;
                                        for (var j = 0; j < 6; j++)
                                        {
                                            if (RevIndex[k] == nd.Nr * 6 + j)
                                            {
                                                temp[j + 1] = ld.Reactions[k - MatrixPart, 0];
                                                k++;
                                                if (k == RevIndex.Length)
                                                {
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                temp[j + 1] = 0;
                                            }
                                        }

                                        break;
                                    }
                                }

                                TableRow(temp);
                            }
                            //for (int i = 0; i < ld.Reactions.RowCount; i += 6)
                            //{
                            //    int j = 0;
                            //    temp[j++] = i + 1;
                            //    for (int k = 0; k < 6; k++)
                            //        temp[j++] = ld.Reactions[i + k, 0];
                            //    TableRow(temp);
                            //}
                        }
                    );
                    if (deltaX >= 0 && ld.MinMaxForce != null)
                    {
                        Table(new[]
                            {
                                new[] {"Peak frame member internal forces (local)"},
                                new[] {"Mbr", "", "Forces", "Moments"},
                                new[] {"", "Node", "X", "Y", "Z", "X", "Y", "Z"}
                            }, new[] { 1, 1, 3, 3 }, () =>
                            {
                                var temp = new object[8];
                                for (var i = 0; i < ld.MinMaxForce?.RowCount; i += 2)
                                    for (var j = 0; j < 2; j++)
                                    {
                                        var m = 0;
                                        temp[m++] = i / 2 + 1;
                                        temp[m++] = j == 0 ? "max" : "min";
                                        for (var k = 0; k < 6; k++)
                                        {
                                            temp[m++] = ld.MinMaxForce[i + j, k];
                                        }

                                        TableRow(temp);
                                    }
                            }
                        );
                    }
                } // end load cases output

                if (Param.DynamicModesCount > 0)
                {
                    BlankLine();
                    WriteString("large", "Modal analysis results");
                    Table(new[]
                        {
                            new[] {"Nodal masses (diagonal of the mass matrix) (global)"},
                            new[] {"Node", "Mass", "Inertias"},
                            new[] {"", "X", "Y", "Z", "X", "Y", "Z"}
                        }, 
                        new[] { 1, 3, 3 }, () =>
                        {
                            var temp = new object[7];
                            for (var i = 0; i < DoF; i += 6)
                            {
                                var k = 0;
                                temp[k++] = i + 1;
                                for (var j = 0; j < 6; j++)
                                {
                                    temp[k++] = M[i + j, i + j];
                                }

                                TableRow(temp);
                            }
                        }
                    );
                    var numModes = Math.Min(Param.DynamicModesCount, DoF - MinRestraints);

                    BlankLine();
                    WriteString("large", "Natural frequencies and mass normalized mode shapes");

                    for (var mode = 0; mode < ModParticFactor.RowCount; mode++)
                    {
                        Table(new[]
                            {
                                new[] {$"Mode {mode + 1}"},
                                new[] {"Node", "Displacements", "Rotations"},
                                new[] {"", "X", "Y", "Z", "X", "Y", "Z"}
                            }, new[] { 1, 3, 3 }, () =>
                            {
                                var temp = new object[7];
                                for (var i = 0; i < Nodes.Count; i++)
                                {
                                    var k = 0;
                                    temp[k++] = Nodes[i].Nr + 1;
                                    for (var j = 0; j < 6; j++)
                                    {
                                        temp[k++] = Eigenvector[i * 6 + j, mode];
                                    }

                                    TableRow(temp);
                                }
                            }
                        );
                        Table(new[]
                            {
                                null,
                                new[] {"Freq.\n(Hz)", "Period\n(sec)", "Modal Participation\nfactor", ""},
                                new[] {"", "", "X", "Y", "Z", "", ""}
                            }, 
                            new[] { 1, 1, 3, 2 }, () =>
                                {
                                    var temp = new object[5];
                                    temp[0] = eigenFreq[mode];
                                    temp[1] = 1d / eigenFreq[mode];
                                    for (var i = 0; i < 3; i++)
                                    {
                                        temp[2 + i] = ModParticFactor[mode, 1];
                                    }

                                    TableRow(temp);
                                }
                        );
                        BlankLine();
                    } // numModes

                    var FreqThreshold = Math.Sqrt(Sq(Math.PI * 2d * eigenFreq[numModes - 1])) / (2d * Math.PI);
                    WriteString("", $"There are {numModes} below {FreqThreshold} Hz. {Param.DynamicModesCount - numModes} were not found.");
                } // if Nm

                WriteString("", $"Matrix iterations: {iter}");

                foreach (var err in Errors)
                {
                    TableRow(new object[] { err });
                }
            }
            catch (Exception ex)
            {
                Lg("Error writing Excel sheet");
                throw ex;
            }
        } // end print
    } // end class Glaucon
}
