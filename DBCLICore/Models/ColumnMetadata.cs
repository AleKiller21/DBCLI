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
        public const int NameSize = 50;
        public const int TypeNameSize = 7; //Espacio para guardar el nombre del tipo

        public ColumnMetadata()
        {
            Name = new char[NameSize];
        }

        public static int Size()
        {
            return NameSize + sizeof(int) + TypeNameSize;
        }
    }
}
