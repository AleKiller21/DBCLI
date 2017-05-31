using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlParser.SyntaxAnalyser.Nodes.TypeNodes;

namespace DBCLICore.Models
{
    public class ColumnMetadata
    {
        public char[] Name;
        public TypeNode Type;
        public int NameSize;

        public ColumnMetadata()
        {
            NameSize = 50;
            Name = new char[NameSize];
        }
    }
}
