using SqlParser.SyntaxAnalyser.Nodes.TypeNodes;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes
{
    public class ColumnNode
    {
        public IdNode Name;
        public TypeNode Type;
    }
}