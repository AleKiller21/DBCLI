using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class IdExpectedException : ParserException
    {
        public IdExpectedException(int row, int col) : base($"Id token expected at row {row} column {col}.")
        {
        }
    }
}