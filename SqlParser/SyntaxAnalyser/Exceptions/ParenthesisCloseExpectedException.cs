using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class ParenthesisCloseExpectedException : ParserException
    {
        public ParenthesisCloseExpectedException(int row, int col) : base($"')' token expected at row {row} column {col}.")
        {
        }
    }
}