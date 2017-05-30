namespace SqlParser.SyntaxAnalyser.Nodes.LiteralNodes
{
    public class DoubleNode : LiteralNode
    {
        private readonly double _value;

        public DoubleNode(double value)
        {
            _value = value;
        }

        public override dynamic Evaluate()
        {
            return _value;
        }
    }
}