#region FileHeader
// Project: Glaucon4
// Filename:   BuildCSV.cs
// Last write: 4/30/2023 2:29:49 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Terwiel.Glaucon
{

    /// <summary>
    /// This part of the class MainViewModel has the routines to
    /// print the calculated data to an HTML file, which can be viewed
    /// with your default browser
    /// </summary>
    public partial class Glaucon
    {
        /// <summary>
        /// Builds an HTML page
        /// </summary>
        /// <param name="outputFilename">The full path and name of the HTML file that is generated</param>
        /// <returns></returns>
        public void BuildCSV(string outputFilename)
        {
#if DEBUG
            var method = MethodBase.GetCurrentMethod().Name;
#endif
            using (var w = new StreamWriter(outputFilename))
            {
                var df = new DoubleFormatter();

                // our own special output format:
                string Sf(double v)
                {
                    return string.Format($"{v:E4}");
                }

                void BlankLine()
                {
                    w.Write("\n");
                }

                void WriteString(string style, string s)
                {
                    w.Write($"{s};\n");
                }

                void TableNoHeader(List<object[]> data)
                {
                    foreach (var d in data)
                    {
                        w.Write($"{d[0]};;;;");
                        w.Write($"{(d[1] is double ? d[1] : d[1].ToString())}\n");
                    }
                }

                void Table(string[][] h, int[] pos, Action WriteTableBody)
                {
                    Debug.Assert(h[1].Length == pos.Length,
                        $"{method}: second header row and column widths array not of equal length");
                    w.Write("\n");
                    for (var k = 0; k < 3; k++)
                    {
                        if (h[k] != null)
                        {
                            var cells = h[k].Length;

                            for (var i = 0; i < cells; i++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        w.Write($"{tableNr++}; {h[0][0]};\n");
                                        break;
                                    case 1:
                                        //h[1][i].Replace('Δ','d').Replace('α','a');

                                        w.Write($"{h[1][i].Replace('Δ', 'd').Replace('α', 'a')}; ");
                                        break;
                                    case 2:
                                        w.Write($"{h[2][i]}; ");
                                        break;
                                } // switch                         
                            }

                            w.Write("\n");
                        }
                    }

                    WriteTableBody();
                    tableNr++;
                }

                void Tr(string style, object[] v)
                {
                    foreach (var _v in v)
                    {
                        w.Write($"{_v}; ");
                    }

                    w.Write("\n");
                }

                void TableRow(object[] v)
                {
                    Tr("", v);
                }

                WriteOutput(WriteString, TableNoHeader, TableRow, BlankLine, Table, Sf);
                w.Close();
            }
        }
    }
}
