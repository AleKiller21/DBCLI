using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class DeleteKeywordExpectedException : ParserException
    {
        public DeleteKeywordExpectedException(int row, int col) : base($"'delete' keyword expected at row {row} column {col}.")
        {
        }
    }
}