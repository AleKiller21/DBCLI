using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class ParenthesisOpenExpectedException : ParserException
    {
        public ParenthesisOpenExpectedException(int row, int col) : base($"'(' token expected at row {row} column {col}.")
        {
        }
    }
}