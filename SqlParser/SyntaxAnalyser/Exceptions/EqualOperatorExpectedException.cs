using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class EqualOperatorExpectedException : ParserException
    {
        public EqualOperatorExpectedException(int row, int col) : base($"'=' operator expected at row {row} column {col}.")
        {
        }
    }
}