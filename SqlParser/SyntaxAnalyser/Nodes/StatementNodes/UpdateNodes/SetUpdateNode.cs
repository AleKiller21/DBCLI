using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.UpdateNodes
{
    public class SetUpdateNode
    {
        public IdNode Column;
        public LiteralNode Value;
    }
}