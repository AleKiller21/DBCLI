using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes
{
    public class DropDatabaseNode : DropObjectNode
    {
        public StringNode Path;
        public override void Interpret()
        {
            throw new NotImplementedException();
        }
    }
}
