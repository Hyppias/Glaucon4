#region FileHeader
// Project: Glaucon4
// Filename:   BuildLatex.cs
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
using System.Text;
using System.Threading;

namespace Terwiel.Glaucon {

/// <summary>
/// This part of the class MainViewModel has the routines to
/// print the calculated data to an HTML file, which can be viewed
/// with your default browser
/// </summary>
public partial class Glaucon
{
    private class Row
    {
        public readonly string Label;
        public readonly object Value;
        public readonly int Decimals;
        public readonly string Units;
        public readonly string Remark;
        public readonly bool? Ok;
        public bool Scientific = false;

        public Row(string label, object value, int decimals, string units, string remark = null, bool? ok = null)
        {
            Label = label;
            Value = value;
            Decimals = decimals;
            Units = units;
            Remark = remark;
            Ok = ok;
        }
    }

    /// <summary>
    /// Builds an HTML page
    /// </summary>
    /// <param name="outputFilename">The full path and name of the HTML file that is generated</param>
    /// <returns></returns>
    public void BuildLatex(string outputFilename)
    {
#if DEBUG
        var method = MethodBase.GetCurrentMethod().Name;
#endif
        var Ltx = new StringBuilder();

        var df = new DoubleFormatter();

        // our own special output format:
        string Sf(double v)
        {
            return string.Format(df, $"{{0:H{Param.Decimals}}}", v);
        }

        void BlankLine()
        {
            Ltx.Append("\\\\\n");
        }

        void TableNoHeader(List<object[]> data)
        {
            bool landscape = false;
            var ltx = new StringBuilder();
            BlankLine();
            ltx.AppendLine("\\newgeometry{left=1.3cm,right=1.3cm,top=4.2cm,bottom=4.0cm}");
            ltx.AppendLine("\\AddToShipoutPicture{\\BackgroundStructureNoMargin}");
            ltx.Append(
                (landscape ? "\\begin{landscape}\n" : string.Empty)
                + $"\\begin{{longtable}}{{rr}}\n"
                + $"\\toprule[2pt]\n");

            foreach (var d in data)
            {
                ltx.Append($"{d[0]} &");
                var _d = d[1];
                if (_d is double || _d is double)
                {
                    ltx.Append($"{((double) _d)} &");
                }
                else
                {
                    ltx.Append($"{_d} &");
                }
            }
            ltx.Append(ltx.ToString().TrimEnd('&') + "\\\\\n");

            Ltx.Append(ltx);

            Ltx.AppendLine("\\bottomrule[1pt]\n" +
                "\\end{longtable}\n" +
                (landscape ? "\\end{landscape}" : "") +
                "\n\\restoregeometry\n" +
                "\\AddToShipoutPicture{\\BackgroundStructure}");

            tableNr++;
        }

        void Table(string[][] h, int[] pos, Action WriteTableBody)
        {
            Debug.Assert(h[1].Length == pos.Length,
                $"{method}: second header row and column widths array not of equal length");
            bool landscape = false;
            var ltx = new StringBuilder();
            BlankLine();
            ltx.AppendLine("\\newgeometry{left=1.3cm,right=1.3cm,top=4.2cm,bottom=4.0cm}");
            ltx.AppendLine("\\AddToShipoutPicture{\\BackgroundStructureNoMargin}");
            ltx.Append(
                (landscape ? "\\begin{landscape}\n" : string.Empty)
                + $"\\begin{{longtable}}{{{"rrrrrrrrrrrrrrrrrrrr".Substring(0, pos.Length)}}}\n"
                + $"\\toprule[2pt]\n");

            for (var k = 0; k < 3; k++)
            {
                if (h[k] != null)
                {
                    var cells = h[k].Length;

                    //w.Write(k > 0 ? "<tr class=\"green\">\n" : "<tr>\n");
                    for (var i = 0; i < cells; i++)
                    {
                        switch (k)
                        {
                            case 0:
                                ltx.Append($"\\multicol{{r}}{{{h[2].Length}}}{h[0][0]} &");
                                break;
                            case 1:
                                ltx.Append($"\\multicol{{r}}{{{pos[i]}}}{h[1][i]} &");
                                break;
                            case 2:
                                ltx.Append($"{h[2][i]} &");
                                break;
                        } // switch                         
                    }

                    ltx.Append(ltx.ToString().TrimEnd('&') + "\\\\\n");
                }
            }

            Ltx.Append(ltx);

            Ltx.AppendLine("\\bottomrule[1pt]\n" +
                "\\end{longtable}\n" +
                (landscape ? "\\end{landscape}" : "") +
                "\n\\restoregeometry\n" +
                "\\AddToShipoutPicture{\\BackgroundStructure}");

            WriteTableBody();

            tableNr++;
        }

        void Tr(string style, object[] v)
        {
            var ltx = new StringBuilder();

            foreach (var v_ in v)
            {
                if (v_ is double || v_ is double)
                {
                    ltx.Append($"{Convert.ToDouble(v_)} &");
                }
                else
                {
                    ltx.Append($"{v_} &");
                }
            }

            Ltx.Append(ltx.ToString().TrimEnd('&') + "\\\\\n");
        }

        void WriteString(string style, string s)
        {
            switch (style)
            {
                case "small":
                    Ltx.Append($"{{\\small {s}\\par}}");
                    break;
                case "large":
                    Ltx.Append($"{{\\Large {s}\\par}}");
                    break;
                case "h1":
                    Ltx.Append($"{{\\LARGE {s}\\par}}");
                    break;
                default:
                    break;
            }
            Ltx.AppendLine(s);
        }

        // for the table rows:
        void TableRow(object[] v)
        {
            Tr("", v);
        }

        Ltx.Append(
            "\\documentclass[11pt,a4paper]{article}\n"
            + "\\usepackage[per-mode=symbol]{siunitx}\n"
            + "\\usepackage{marginnote}\n"
            + "\\usepackage{wallpaper}\n"
            + "\\usepackage{lastpage}\n"
            + "\\usepackage[left=1.3cm,right=4.6cm,top=1.8cm,bottom=4.0cm,marginparwidth=3.4cm]{geometry}\n"
            + "\\usepackage{amsmath}\n"
            + "\\usepackage{amssymb}\n"
            + "\\usepackage{numprint}\n"
            + "\\usepackage{gensymb}\n"
            + "\\usepackage{xcolor, colortbl}\n"
            //+ "\\usepackage{num}\n" 
            //+ "\\usepackage{hyperref}\n" 
            + "\\usepackage{pdflscape}\n"
            + "\\usepackage{multirow}\n"
            + "\\usepackage{rotating}\n"
            + "\\usepackage{array}\n"
            + "\\usepackage{booktabs}\n"
            + "\\usepackage{subfigure}\n"
            + "\\usepackage{caption}\n"
            + $"\\usepackage[{(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.Equals("NL") ? "dutch" : "british")}]{{datetime2}}\n"
            + "\\usepackage{longtable}\n"
            + "\\usepackage{tikz}\n"
            + "\\usetikzlibrary{arrows.meta}\n"
            + "\\usepackage{fancyhdr}\n"
            + "\\setlength{\\headheight}{80pt}\n"
            + "\\pagestyle{fancy}\\fancyhf{}\n"
            + "\\renewcommand{\\headrulewidth}{0pt}\n"
            + "\\setlength{\\parindent}{0cm}\n"
            + "\\setlength{\\abovetopsep}{1ex}\n"
            + "\\newcommand{\\tab}{\\hspace*{2em}}\n"
            + "\\DeclareMathAlphabet{\\mathcal}{OMS}{cmsy}{m}{n}\n"
            + "\\newcommand\\BackgroundStructure{\n"
            + "	\\setlength{\\unitlength}{1mm}\n"
            + "	\\setlength\\fboxsep{0mm}\n"
            + "	\\setlength\\fboxrule{0.5mm}\n"
            + "	\\put(10, 10){\\fcolorbox{black}{blue!10}{\\framebox(155,247){}}}\n"
            + "	\\put(165, 10){\\fcolorbox{black}{blue!10}{\\framebox(37,247){}}}\n"
            + "	\\put(10, 262){\\fcolorbox{black}{white!10}{\\framebox(192, 25){}}}\n"
            + "	\\put(160, 263){\\includegraphics[height=23mm,keepaspectratio]{logo}}\n"
            + "}\n" // end background structure
            + "\\newcommand\\BackgroundStructureNoMargin{\n"
            + "	\\setlength{\\unitlength}{1mm}\n"
            + "	\\setlength\\fboxsep{0mm}\n"
            + "	\\setlength\\fboxrule{0.5mm}\n"
            + "	\\put(10, 10){\\fcolorbox{black}{blue!10}{\\framebox(192,247){}}}\n"
            //  + "	\\put(165, 10){\\fcolorbox{black}{blue!10}{\\framebox(37,247){}}}\n"
            + "	\\put(10, 262){\\fcolorbox{black}{white!10}{\\framebox(192, 25){}}}\n"
            + "	\\put(160, 263){\\includegraphics[height=23mm,keepaspectratio]{logo}}\n"
            + "}\n" // end background structure
            + "%	HEADER INFORMATION\n"
            + "\\fancyhead[L]{\\begin{tabular}{l r | l r}\n"
            + $"\\textbf{{Project}} &  & \\textbf{{Pag.}} & \\thepage/\\pageref{{LastPage}} \\\\\n"
            + $" &  & \\textbf{{Date & \\today \\\\\n"
            + $" &  & \\textbf{{Program version & {Assembly.GetExecutingAssembly().GetName().Version}\\\\\n"
            + "\\end{tabular}}\n"
            + "\\newcolumntype{3}{>{\\centering\\arraybackslash}m{3cm}}\n"
            + "\\newcolumntype{R}[1]{>{\\begin{turn}{90}\\begin{minipage}{#1}\\scriptsize}l%\n"
            + "<{\\end{minipage}\\end{turn}}%\n"
            + "}\n"
            //+ "\\newcolumntype{H}{>{\\setbox0=\\hbox\\bgroup}c<{\\egroup}@{}}\n"
            + "% Look here for the logo file and other graphics:\n"
        );
        Ltx.Append(
            // BEGIN DOCUMENT
            "\\begin{document}\n"
            + "\\AddToShipoutPicture{\\BackgroundStructure}\n"
            + $"\\section{{Result}}\n");

        Ltx.Append($"{ProgramName} v. {ProgramVersion}\\\\\n"
            + "A program to compute 2D and 3D trusses and frames\\\\\n"
            + "By E.H. Terwiel, 2020, based on:\\\\\n"
            + "Frame3DD: GPL Copyright (C) 1992-2015, by Henri P. Gavin, http://frame3dd.sf.net/ \\\\\n"
            //WriteString($"Uses {Control.LinearAlgebraProvider.ToString()}");
            + $"Date/time: {DateTime.Now.ToString(culture.DateTimeFormat.FullDateTimePattern)}\\\\\n"
            + Title
            + $"The {(Dim3 ? "Z" : "Y")}-axis is vertical.\\\\\n");

        Ltx.AppendLine("Input data");

       
        WriteOutput(WriteString, TableNoHeader, TableRow, BlankLine, Table, Sf);

        Ltx.Append("</body>\n</html>");
        File.WriteAllText(outputFilename, Ltx.ToString());
    }
}
}
