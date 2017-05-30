namespace SqlParser.SyntaxAnalyser.Nodes.LiteralNodes
{
    public class IntNode : LiteralNode
    {
        private readonly int _value;

        public IntNode(int value)
        {
            _value = value;
        }

        public override dynamic Evaluate()
        {
            return _value;
        }
    }
}