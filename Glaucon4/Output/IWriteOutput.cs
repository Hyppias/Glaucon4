
using System;
using System.Collections.Generic;

namespace Terwiel.Glaucon
{
    interface IWriteOutput
    {
        void Table(string[][] h, int[] pos, Action WriteTableBody);
        void BlankLine();
        void TableNoHeader(List<object[]> data);
        void WriteString(string style, string v);
        void Tr(string style, object[] v);
        void TableRow(object[] v);

    }
}
