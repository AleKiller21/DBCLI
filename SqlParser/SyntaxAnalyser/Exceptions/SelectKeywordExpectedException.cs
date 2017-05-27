using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class SelectKeywordExpectedException : ParserException
    {
        public SelectKeywordExpectedException(int row, int col) : base($"'select' keyword expected at row {row} column {col}.")
        {
        }
    }
}