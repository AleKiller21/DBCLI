using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes
{
    public abstract class DropObjectNode : StatementNode
    {
        public IdNode Name;
    }
}
