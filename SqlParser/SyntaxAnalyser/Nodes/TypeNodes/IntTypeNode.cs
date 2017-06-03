namespace SqlParser.SyntaxAnalyser.Nodes.TypeNodes
{
    public class IntTypeNode : TypeNode
    {
        public IntTypeNode()
        {
            Size = 4;
        }

        public override string ToString()
        {
            return "int";
        }
    }
}
