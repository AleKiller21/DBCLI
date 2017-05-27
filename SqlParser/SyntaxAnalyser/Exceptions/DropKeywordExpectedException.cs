using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class DropKeywordExpectedException : ParserException
    {
        public DropKeywordExpectedException(int row, int col) : base($"'drop' keyword expected at row {row} column {col}.")
        {
        }
    }
}