using System.Text.RegularExpressions;

namespace SqlParser.SyntaxAnalyser.Nodes.LiteralNodes
{
    public class StringNode : LiteralNode
    {
        private readonly string _value;

        public StringNode(string value)
        {
            _value = Regex.Replace(value, "\"", string.Empty); ;
        }

        public override dynamic Evaluate()
        {
            return _value;
        }
    }
}