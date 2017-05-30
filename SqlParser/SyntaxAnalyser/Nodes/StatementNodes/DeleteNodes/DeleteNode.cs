using SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DeleteNodes
{
    public class DeleteNode : StatementNode
    {
        public IdNode SourceTable;
        public ConditionalExpressionNode Selection;

        public override void Interpret()
        {
            throw new System.NotImplementedException();
        }
    }
}