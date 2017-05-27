
namespace SqlParser
{
    public class Token
    {
        public readonly string Lexeme;
        public readonly TokenType Type;
        public readonly int Row;
        public readonly int Col;

        public Token(string lexeme, TokenType type, int row, int col)
        {
            Lexeme = lexeme;
            Type = type;
            Row = row;
            Col = col;
        }
    }
}
