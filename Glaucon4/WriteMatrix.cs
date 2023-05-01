#region FileHeader
// Project: Glaucon4
// Filename:   WriteMatrix.cs
// Last write: 4/30/2023 2:29:41 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader


using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
#if DEBUG
        private static int matrixnr;
#endif

        [Conditional("DEBUG")]
        public static void WriteMatrix(bool wrt, string prefix, string filename, string matrixName, DenseMatrix m)
        {
            if (!wrt)
            {
                return;
            }
            var debugPath = Param.OutputPath;
            var sb = new StringBuilder();
            sb.Append(matrixName + (matrixnr++) + " = [" + Environment.NewLine);
            for (var i = 0; i < m.RowCount; i++)
            {
                for (var j = 0; j < m.ColumnCount; j++)
                {
                    sb.Append(m[i, j].AlmostEqual(0.0) ? "          0          " : $"{m[i, j]:E12} ");
                }

                sb.Append(";" + Environment.NewLine);
            }

            sb.Append("];" + Environment.NewLine);
            if (!Directory.Exists(debugPath))
            {
                Directory.CreateDirectory(debugPath);
            }

            using (var sw1 = File.AppendText(debugPath + prefix + filename))
            {
                sw1.Write(sb.ToString());
                sw1.Flush();
            }
        }

        [Conditional("DEBUG")]
        public static void WriteVector(bool wrt, string prefix, string filename, string vectorName, DenseVector m)
        {
            if (!wrt)
            {
                return;
            }
            var debugPath = Param.OutputPath;
            var sb = new StringBuilder();
            sb.Append(vectorName + (matrixnr++) + " = [" + Environment.NewLine);
            foreach (var m_ in m)
            {
                sb.Append($"{m_:E12} ");
            }

            sb.Append("];" + Environment.NewLine);
            if (!Directory.Exists(debugPath))
            {
                Directory.CreateDirectory(debugPath);
            }

            File.WriteAllText(debugPath + prefix + filename, sb.ToString());
            using (var sw_ = File.AppendText(debugPath + prefix + filename))
            {
                sw_.Write(sb.ToString());
                sw_.Flush();
            }
        }
    }
}
