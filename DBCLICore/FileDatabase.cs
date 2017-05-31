using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DBCLICore
{
    public class FileDatabase
    {
        public FileDatabaseStructures Structures;

        public FileDatabase()
        {
            Structures = new FileDatabaseStructures();
        }

        public void CreateDatabase(CreateDatabaseNode node)
        {
            Database.CreateDatabase(node.Name.ToString(), node.Size, node.Unit, Structures);
        }

        public void DropDatabase(DropDatabaseNode node)
        {
            Database.DropDatabase(node.Name.ToString());
        }
    }
}
