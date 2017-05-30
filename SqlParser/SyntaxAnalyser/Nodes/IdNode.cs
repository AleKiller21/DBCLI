namespace SqlParser.SyntaxAnalyser.Nodes
{
    public class IdNode : StatementNode
    {
        private readonly string _lexeme;

        public IdNode(string lexeme)
        {
            _lexeme = lexeme;
        }

        public override string ToString()
        {
            return _lexeme;
        }

        public override void Interpret()
        {
            throw new System.NotImplementedException();
        }
    }
}