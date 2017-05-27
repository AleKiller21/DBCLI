using System;

namespace SqlParser.SyntaxAnalyser.Exceptions
{
    [Serializable]
    internal class IntLiteralExpectedException : Exception
    {
        public IntLiteralExpectedException(int row, int col) : base($"int literal expected at row {row} column {col}.")
        {
        }
    }
}