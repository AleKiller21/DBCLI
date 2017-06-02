using System;
using System.IO;
using DBCLICore.Exceptions;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DBCLICore
{
    public class FileDatabase
    {
        public FileDatabaseStructures Structures;
        private Database _databaseManager;
        private bool _connection;

        public FileDatabase()
        {
            _connection = false;
            _databaseManager = new Database();
        }

        public void CreateDatabase(CreateDatabaseNode node)
        {
            try
            {
                _databaseManager.CreateDatabase(node.Name.ToString(), node.Size, node.Unit);
                Console.WriteLine($"{node.Name} database has been created successfully.");
            }
            catch (NotDivisibleByTwoException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occurred. More info below:");
                Console.WriteLine(e.Message);
            }
        }

        public void DropDatabase(DropDatabaseNode node)
        {
            try
            {
                _databaseManager.DropDatabase(node.Name.ToString());
                Console.WriteLine($"{node.Name} database has been deleted.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ConnectDatabase(ConnectionNode node)
        {
            try
            {
                Structures = _databaseManager.ConnectDatabase(node.DatabaseName.ToString());
                _connection = true;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("No such database exists!");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occurred! Here is more info:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
