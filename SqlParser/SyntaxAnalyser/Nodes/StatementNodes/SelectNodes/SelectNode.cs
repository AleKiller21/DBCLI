using System;
using System.Collections.Generic;
using SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.SelectNodes
{
    public class SelectNode : StatementNode
    {
        public List<IdNode> Columns;
        public IdNode SourceTable;
        public ConditionalExpressionNode Selection;

        public override void Interpret()
        {
            throw new NotImplementedException();
        }
    }
}
