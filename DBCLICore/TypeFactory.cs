using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlParser.SyntaxAnalyser.Nodes.TypeNodes;

namespace DBCLICore
{
    public static class TypeFactory
    {
        public static TypeNode GetType(string typeName)
        {
            switch (typeName)
            {
                case "double":
                    return new DoubleTypeNode();

                case "int":
                    return new IntTypeNode();

                default:
                    return new StringTypeNode();
            }
        }
    }
}
