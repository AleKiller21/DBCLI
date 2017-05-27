using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class IntoKeywordExpectedException : ParserException
    {
        public IntoKeywordExpectedException(int row, int col) : base($"'into' keyword expected at row {row} column {col}.")
        {
        }
    }
}