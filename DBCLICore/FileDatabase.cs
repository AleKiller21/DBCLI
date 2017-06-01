using System;
using System.IO;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DBCLICore
{
    public class FileDatabase
    {
        public FileDatabaseStructures Structures;
        private Database _databaseManager;

        public FileDatabase()
        {
            _databaseManager = new Database();
        }

        public void CreateDatabase(CreateDatabaseNode node)
        {
            _databaseManager.CreateDatabase(node.Name.ToString(), node.Size, node.Unit);
        }

        public void DropDatabase(DropDatabaseNode node)
        {
            _databaseManager.DropDatabase(node.Name.ToString());
        }

        public void ConnectDatabase(ConnectionNode node)
        {
            try
            {
                Structures = _databaseManager.ConnectDatabase(node.DatabaseName.ToString());
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
