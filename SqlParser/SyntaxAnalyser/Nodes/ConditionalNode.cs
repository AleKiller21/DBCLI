using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;

namespace SqlParser.SyntaxAnalyser.Nodes
{
    public abstract class ConditionalNode
    {
        public IdNode LeftOperand;
        public LiteralNode RightOperand;
    }
}