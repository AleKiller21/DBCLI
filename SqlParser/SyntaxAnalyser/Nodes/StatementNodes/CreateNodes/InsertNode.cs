using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes
{
    public class InsertNode : StatementNode
    {
        public IdNode TargetTable;
        public List<ValueNode> Values;

        public override void Interpret()
        {
            throw new NotImplementedException();
        }
    }
}
