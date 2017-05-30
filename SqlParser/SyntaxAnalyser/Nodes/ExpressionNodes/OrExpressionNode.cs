﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes
{
    public class OrExpressionNode : ConditionalExpressionNode
    {
        public ConditionalExpressionNode LeftOperand;
        public ConditionalExpressionNode RightOperand;
    }
}
