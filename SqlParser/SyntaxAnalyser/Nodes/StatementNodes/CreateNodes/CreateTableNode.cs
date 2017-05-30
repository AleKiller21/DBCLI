using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes
{
    public class CreateTableNode : CreateObjectNode
    {
        public List<ColumnNode> Columns;

        public override void Interpret()
        {
            throw new NotImplementedException();
        }
    }
}
