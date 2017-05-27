using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class UpdateKeywordExpectedException : ParserException
    {
        public UpdateKeywordExpectedException(int row, int col) : base($"'update' keyword expected at row {row} column {col}.")
        {
        }
    }
}