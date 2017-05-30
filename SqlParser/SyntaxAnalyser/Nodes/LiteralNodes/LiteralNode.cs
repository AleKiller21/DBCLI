namespace SqlParser.SyntaxAnalyser.Nodes.LiteralNodes
{
    public abstract class LiteralNode
    {
        public abstract dynamic Evaluate();
    }
}