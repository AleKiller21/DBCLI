using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class ValuesKeywordExpectedException : ParserException
    {
        public ValuesKeywordExpectedException(int row, int col) : base($"'values' keyword expected at row {row} column {col}.")
        {
        }
    }
}