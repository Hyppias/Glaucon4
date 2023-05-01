#region FileHeader
// Project: Glaucon4
// Filename:   BuildHTML.cs
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
        public void BuildHTML(string outputFilename)
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
                    return string.Format(df, $"{{0:H{Param.Decimals}}}", v);
                }

                void BlankLine()
                {
                    w.Write("<br/>");
                }

                void TableNoHeader(List<object[]> data)
                {
                    w.Write("<table>\n");

                    foreach (var d in data)
                    {
                        w.Write($"<tr>\n<td><p class=\"left\">{d[0]}</p></td>\n");
                        var _d = d[1];
                        if (_d is double || _d is double)
                        {
                            w.Write($"<td><p>{Sf((double)_d)}</p></td>\n</tr>\n");
                        }
                        else
                        {
                            w.Write($"<td><p>{_d}</p></td>\n</tr>\n");
                        }
                    }

                    w.Write("</table>\n");
                }

                void Table(string[][] h, int[] pos, Action WriteTableBody)
                {
                    Debug.Assert(h[1].Length == pos.Length,
                        $"{method}: second header row and column widths array not of equal length");
                    BlankLine();
                    w.Write($"<table id=\"Table_{tableNr}\" class=\"border_black\">\n" +
                        "<thead>\n");
                    for (var k = 0; k < 3; k++)
                    {
                        if (h[k] != null)
                        {
                            var cells = h[k].Length;

                            w.Write(k > 0 ? "<tr class=\"green\">\n" : "<tr>\n");
                            for (var i = 0; i < cells; i++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        w.Write($"<td colspan=\"{h[2].Length}\"><h2>{h[0][0]}</h2></td>\n");
                                        break;
                                    case 1:
                                        w.Write($"<td colspan=\"{pos[i]}\"><h3>{h[1][i]}</h3></td>\n");
                                        break;
                                    case 2:
                                        w.Write($"<td><p>{h[2][i]}</p></td>\n");
                                        break;
                                } // switch                         
                            }

                            w.Write("</tr>\n");
                        }
                    }

                    w.Write(
                        "</thead>\n" +
                        "<tbody>\n");

                    WriteTableBody();
                    w.Write("</tbody>\n</table>\n");
                    tableNr++;
                }

                void Tr(string style, object[] v)
                {
                    w.Write("<tr>\n");

                    foreach (var v_ in v)
                    {
                        if (v_ is double || v_ is double)
                        {
                            w.Write($"<td><p class=\"{style} right\">{Sf(Convert.ToDouble(v_))}</p></td>\n");
                        }
                        else
                        {
                            w.Write($"<td><p class=\"{style} center\">{v_}</p></td>\n");
                        }
                    }

                    w.Write("</tr>\n");
                }

                void WriteString(string style, string s)
                {
                    w.Write($"<p class=\"{style} left\">{s}</p>\n");
                }

                // for the table rows:
                void TableRow(object[] v)
                {
                    Tr("", v);
                }

                w.Write(
                    "<!DOCTYPE html>\n" +
                    "<html>\n" +
                    "<head>\n" +
                    "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>\n" +
                    "<style type=\"text/css\">\n" +
                    "p { text-align:left; margin-top:0.05cm; margin-bottom:0.05cm; margin-left:0.2cm; margin-right:0.2cm;\n" +
                    "   line-height: 1em;}\n" +
                    ".large {font-weight:bold;font-size:large;}" +
                    "td, td p {white-space:nowrap; }\n" +
                    "h1 { color: blue; text-align:left; font-size:x-large;}\n" +
                    "h2 { color: black; text-align:center; font-size:large;}\n" +
                    "h3 { color: black; text-align:center; font-size:medium;}\n" +
                    "h4 { color: black; text-align:center; font-size:normal;margin-left:1cm;margin-right:1cm;}\n" +
                    "table , tr , td { page-break-inside: avoid;}\n" +
                    "span.bold {font-weight:bold;font-size:large;}\n" +
                    "table.border_black { border: black 1px solid; }\n" +
                    "table.border_none { border: none; }\n" +
                    "table {margin-top: 0.5cm;margin-bottom:0.5cm;}\n" +
                    ".right { text-align:right;}\n" +
                    ".left { text-align:left;}\n" +
                    ".red {color:#FF0000 }\n" +
                    "small {font-weight:lighter;}\n" +
                    ".top {margin-top: 0.05cm; top: 5%;}\n" +
                    "thead {background:lightblue; }\n" +
                    "thead tr.green{background:lightgreen; }\n" +
                    "thead tr td p { text-align:center; }\n" +
                    "thead tr th p { text-align:center; }\n" +
                    "tbody tr td p { text-align:right; }\n" +
                    "</style>\n" +
                    "<body>\n");

                WriteOutput(WriteString, TableNoHeader, TableRow, BlankLine, Table, Sf);

                w.Write("</body>\n</html>");
                w.Close();
            }
        }
    }
}
