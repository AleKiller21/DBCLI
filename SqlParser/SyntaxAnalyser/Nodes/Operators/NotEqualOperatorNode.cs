using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser.SyntaxAnalyser.Nodes.Operators
{
    public class NotEqualOperatorNode : ConditionalNode
    {
        public override bool Evaluate(dynamic leftOperandValue)
        {
            return leftOperandValue != RightOperand.Evaluate();
        }
    }
}
