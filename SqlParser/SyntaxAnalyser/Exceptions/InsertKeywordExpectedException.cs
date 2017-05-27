using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class InsertKeywordExpectedException : ParserException
    {
        public InsertKeywordExpectedException(int row, int col) : base($"'insert' keyword expected at row {row} column {col}.")
        {
        }
    }
}