using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class SetKeywordExpectedException : ParserException
    {
        public SetKeywordExpectedException(int row, int col) : base($"'set' keyword expected after update statement at row {row} column {col}.")
        {
        }
    }
}