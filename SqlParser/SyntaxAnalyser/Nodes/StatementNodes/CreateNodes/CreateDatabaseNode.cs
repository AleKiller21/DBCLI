using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes
{
    public class CreateDatabaseNode : CreateObjectNode
    {
        public UnitSize Unit;
        public int Size;
        public StringNode Path;

        public CreateDatabaseNode(StringNode path, UnitSize unit, int size)
        {
            Path = path;
            Unit = unit;
            Size = size;
        }

        public CreateDatabaseNode(IdNode name, UnitSize unit, int size)
        {
            Name = name;
            Unit = unit;
            Size = size;
        }

        public override void Interpret()
        {
            throw new NotImplementedException();
        }
    }
}
