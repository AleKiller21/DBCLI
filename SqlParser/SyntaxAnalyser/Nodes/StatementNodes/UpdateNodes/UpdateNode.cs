using System.Collections.Generic;
using SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.UpdateNodes
{
    public class UpdateNode : StatementNode
    {
        public IdNode SourceTable;
        public List<SetUpdateNode> Updates;
        public ConditionalExpressionNode Selection;

        public override void Interpret()
        {
            throw new System.NotImplementedException();
        }
    }
}