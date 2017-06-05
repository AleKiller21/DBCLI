using System;
using System.IO;
using System.Linq;
using DBCLICore.Exceptions;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.SelectNodes;

namespace DBCLICore
{
    public class FileDatabase
    {
        private DatabaseManager _databaseManager;

        public FileDatabase()
        {
            _databaseManager = new DatabaseManager();
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
                Console.WriteLine($"{node.Name} database has been dropped.");
            }
            catch (SessionActiveException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occurred! Here is more info:");
                Console.WriteLine(e.Message);
            }
        }

        public void ConnectDatabase(ConnectionNode node)
        {
            _databaseManager.ConnectDatabase(node.DatabaseName.ToString());
            //try
            //{
            //    _databaseManager.ConnectDatabase(node.DatabaseName.ToString());
            //    Console.WriteLine($"A session to {node.DatabaseName} has been created.");
            //}
            //catch (SessionActiveException e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            //catch (FileNotFoundException e)
            //{
            //    Console.WriteLine("No such database exists!");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("An error has occurred! Here is more info:");
            //    Console.WriteLine(e.Message);
            //}
        }

        public void DisconnectDatabase()
        {
            try
            {
                _databaseManager.DisconnectDatabase();
                Console.WriteLine("Session has ended.");
            }
            catch (SessionNotCreatedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CreateTable(CreateTableNode node)
        {
            _databaseManager.CreateTable(node);
            Console.WriteLine($"{node.Name} table has been created.");
            //try
            //{
            //    _databaseManager.CreateTable(node);
            //    Console.WriteLine($"{node.Name} table has been created.");
            //}
            //catch (ColumnSizeOutOfRangeException e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            //catch (SessionNotCreatedException e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("An error has occurred! Here is more info:");
            //    Console.WriteLine(e.Message);
            //}
        }

        public void ShowTables()
        {
            try
            {
                Console.WriteLine("");
                Console.WriteLine(
                    $"|{"Table",20}|{"Column",20}|{"Type",20}|{"Type Size",20}|{"Record Size",20}|{"Total Records",20}|");
                Console.WriteLine("_______________________________________________________________________________________________________________________________");
                var prints = _databaseManager.ShowTables();

                foreach (var print in prints)
                {
                    Console.WriteLine(print);
                }
                Console.WriteLine("");
            }
            catch (SessionNotCreatedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ShowSuperBlock()
        {
            try
            {
                var fields = _databaseManager.ShowSuperBlock();
                Console.WriteLine("");
                foreach (var field in fields)
                {
                    Console.WriteLine(field);
                }
                Console.WriteLine("");
            }
            catch (SessionNotCreatedException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void DropTable(DropTableNode node)
        {
            _databaseManager.DropTable(node.Name.ToString());
        }

        public void InsertRecord(InsertNode node)
        {
            _databaseManager.InsertRecords(node);
        }

        public void SelectRecords(SelectNode node)
        {
            var prints = _databaseManager.SelectRecords(node);

            foreach (var print in prints)
            {
                Console.WriteLine(print);
            }
            Console.Write("\n");
        }
    }
}
