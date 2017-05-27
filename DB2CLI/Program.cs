using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlParser;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lex = new Lexer("UPDATE REGIONS\n SET REGION_NAME = \"HOLA\"\n WHERE ID = 5 OR ID = 4;".ToLower());
            var token = lex.GetToken();
            while (token != null)
            {
                Console.WriteLine($"lexeme: {token.Lexeme}, type: {token.Type}, row: {token.Row}, column: {token.Col}");
                token = lex.GetToken();
            }
        }
    }
}
