using System.Collections.Generic;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore.Models
{
    public class Record
    {
        public List<ValueNode> Values;

        public Record()
        {
            Values = new List<ValueNode>();
        }
    }
}