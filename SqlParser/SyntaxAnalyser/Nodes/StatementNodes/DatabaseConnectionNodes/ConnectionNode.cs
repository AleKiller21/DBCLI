namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes
{
    public abstract class ConnectionNode : StatementNode
    {
        public IdNode DatabaseName;
    }
}
