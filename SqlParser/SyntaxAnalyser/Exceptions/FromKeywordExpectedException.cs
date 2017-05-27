namespace SqlParser.SyntaxAnalyser.Exceptions
{
    public class FromKeywordExpectedException : ParserException
    {
        public FromKeywordExpectedException(int row, int col) : base($"'from' keyword expected at row {row} column {col}.")
        {
        }
    }
}