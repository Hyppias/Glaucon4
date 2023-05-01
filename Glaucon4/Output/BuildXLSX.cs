#region FileHeader
// Project: Glaucon4
// Filename:   BuildXLSX.cs
// Last write: 4/30/2023 2:29:50 PM
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Terwiel.Glaucon
{

    /// <summary>
    /// This part of the class MainViewModel has the routines to
    /// print the calculated data to an HTML file, which can be viewed
    /// with your default browser
    /// </summary>
    public partial class Glaucon
    {
        private static readonly DateTime now = DateTime.Now;

        private DateTime uDat = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second,
            DateTimeKind.Utc);

        [NonSerialized]
        private ExcelWorksheet workSheet;

        /// <summary>
        /// Builds an HTML page
        /// </summary>
        /// <param name="outputFilename">The full path and name of the HTML file that is generated</param>
        /// <returns></returns>
        public void BuildExcel(string outputFilename)
        {
#if DEBUG
            var method = MethodBase.GetCurrentMethod().Name;
#endif
            var df = new DoubleFormatter();

            // our own special output format:
            string Sf(double v)
            {
                return v.ToString("E4");
            }

            void BlankLine()
            {
                row++;
            }

            void TableNoHeader(List<object[]> data)
            {
                var cells = (int)(data.Max(d => d[0].ToString().Length) / workSheet.DefaultColWidth) + 1;
                foreach (var d in data)
                {
                    workSheet.Cells[row, 1, row, cells].Merge = true;
                    workSheet.Cells[row, 1].Value = d[0];
                    if (d[1] is double || d[1] is double)
                    {
                        workSheet.Cells[row, cells + 1].Value = Convert.ToDouble(d[1]);
                    }
                    else if (d[1] is int)
                    {
                        workSheet.Cells[row, cells + 1].Value = Convert.ToInt32(d[1]);
                    }
                    else
                    {
                        workSheet.Cells[row, cells + 1].Value = d[1];
                    }

                    row++;
                }
            }

            void Table(string[][] h, int[] pos, Action writeTableBody)
            {
                Debug.Assert(h[1].Length == pos.Length,
                    $"{method}: second header row and column widths array not of equal length");
                row++; // every table starts with an empty row
                for (var k = 0; k < 3; k++)
                {
                    if (h[k] != null)
                    {
                        var cells = h[k].Length;
                        var c = 1;
                        var range = workSheet.Cells[row, 1, row, h[2].Length]; // entire table width
                        for (var i = 0; i < cells; i++)
                        {
                            switch (k)
                            {
                                case 0:
                                    workSheet.Cells[row, 1].Value = $"{tableNr++}: {h[0][0]}";
                                    workSheet.Cells[row, 1].StyleName = "h1";
                                    range.Merge = true;
                                    break;
                                case 1:
                                    range.StyleName = "h2";
                                    range = workSheet.Cells[row, c, row, c + pos[i] - 1];
                                    range.Merge = pos[i] != 1;
                                    workSheet.Cells[row, c].Value = h[1][i];
                                    c += pos[i];
                                    break;
                                case 2:
                                    range.StyleName = "h3";
                                    workSheet.Cells[row, i + 1].Value = h[2][i];
                                    break;
                            } // switch                         
                        }

                        row++;
                    }
                }

                workSheet.Cells[row - 2, 1, row - 1, 1].Merge = true;
                writeTableBody();
                tableNr++;
            }

            void WriteString(string style, string v)
            {
                workSheet.Cells[row, 1, row, 10].Merge = true;
                workSheet.Cells[row, 1].StyleName = style;
                workSheet.Cells[row++, 1].Value = v;
            }

            void Tr(string style, object[] v)
            {
                for (var i = 0; i < v.Length; i++)
                {
                    workSheet.Cells[row, i + 1].StyleName = style;
                    if (v[i] is double || v[i] is double)
                    {
                        workSheet.Cells[row, i + 1].Style.Numberformat.Format = "0.##0E+0";
                    }

                    workSheet.Cells[row, i + 1].Value = v[i];
                }

                row++;
            }

            void TableRow(object[] v)
            {
                Tr("Default", v);
            }

            if (File.Exists(outputFilename))
            {
                File.Delete(outputFilename);
            }

            using (var p = new ExcelPackage(new FileInfo(outputFilename)))
            {
                workSheet = p.Workbook.Worksheets.Add(BaseFile);
                workSheet.DefaultColWidth = 12;
                workSheet.Column(1).Width = 5;
                p.Workbook.Properties.Author = "E.H. Terwiel";
                p.Workbook.Properties.Created = uDat;
                p.Workbook.Properties.AppVersion = "16.00";

                var Default = p.Workbook.Styles.CreateNamedStyle("Default");
                Default.Style.Font.SetFromFont("Times New Roman", 12);
                Default.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                Default.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var H1 = p.Workbook.Styles.CreateNamedStyle("h1");
                H1.Style.Font.SetFromFont("Calibri", 14);
                H1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                H1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                H1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                H1.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                H1.Style.Font.Bold = true;

                var H2 = p.Workbook.Styles.CreateNamedStyle("h2");
                H2.Style.Font.SetFromFont("Calibri", 12);
                H2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                H2.Style.WrapText = true;
                H2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                H2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                H2.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

                var H3 = p.Workbook.Styles.CreateNamedStyle("h3");
                H3.Style.Font.SetFromFont("Calibri", 12);
                H3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                H3.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                H3.Style.Fill.PatternType = ExcelFillStyle.Solid;
                H3.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

                var H4 = p.Workbook.Styles.CreateNamedStyle("h4");
                H4.Style.Font.SetFromFont("Calibri", 14);
                H4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                H4.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                H4.Style.WrapText = false;
                H4.Style.Fill.PatternType = ExcelFillStyle.Solid;
                // Solid must be used with SetColor, else black!
                H4.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

                var Large = p.Workbook.Styles.CreateNamedStyle("Large");
                Large.Style.Font.SetFromFont("Calibri", 14);
                Large.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Large.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Large.Style.Font.Bold = true;

                WriteOutput(WriteString, TableNoHeader, TableRow, BlankLine, Table, Sf);

                p.SaveAs(new FileInfo(outputFilename));
            } // end using
        } // end BuildExcel
    } // end class Glaucon
}
