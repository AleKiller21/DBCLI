namespace SqlParser.SyntaxAnalyser.Nodes.TypeNodes
{
    public class DoubleTypeNode : TypeNode
    {
        public DoubleTypeNode()
        {
            Size = 8;
        }

        public override string ToString()
        {
            return "double";
        }
    }
}
