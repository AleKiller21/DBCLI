using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class CreateKeywordExpectedException : ParserException
    {
        public CreateKeywordExpectedException(int row, int col) : base($"'create' keyword expected at row {row} column {col}.")
        {
        }
    }
}