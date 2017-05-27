using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class DropKeywordExpectedException : Exception
    {
        public DropKeywordExpectedException(int row, int col) : base($"'drop' keyword expected at row {row} column {col}.")
        {
        }
    }
}