using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes
{
    public abstract class CreateObjectNode : StatementNode
    {
        public IdNode Name;
    }
}
