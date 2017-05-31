using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class FileDatabase
    {
        public SuperBlock Super;
        public DirectoryEntry[] Directory;
        public Inode[] Inodes;

        public FileDatabase()
        {
            
        }

        public void CreateDatabase(CreateDatabaseNode node)
        {
            var database = new Database();
            database.CreateDatabase(node.Name.ToString(), node.Size, node.Unit);
        }
    }
}
