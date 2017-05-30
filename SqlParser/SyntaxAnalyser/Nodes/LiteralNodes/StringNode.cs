namespace SqlParser.SyntaxAnalyser.Nodes.LiteralNodes
{
    public class StringNode : LiteralNode
    {
        private readonly string _value;

        public StringNode(string value)
        {
            _value = value;
        }

        public override dynamic Evaluate()
        {
            return _value;
        }
    }
}