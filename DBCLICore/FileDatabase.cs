using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

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
            Database.CreateDatabase(node.Name.ToString(), node.Size, node.Unit);
        }

        public void DropDatabase(DropDatabaseNode node)
        {
            Database.DropDatabase(node.Name.ToString());
        }
    }
}
