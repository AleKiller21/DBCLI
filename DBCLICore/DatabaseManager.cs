using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBCLICore.Exceptions;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class DatabaseManager
    {
        private string _currentDatabase;
        private bool _connection;
        private readonly FileDatabaseWriter _writer;
        private readonly FileDatabaseReader _reader;

        public DatabaseManager()
        {
            _connection = false;
            _writer = new FileDatabaseWriter();
            _reader = new FileDatabaseReader();
        }

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ManagerUtilities.ConvertToBytes(size, unit);

            if (!ManagerUtilities.CheckIfSizeDivisibleByTwo(size)) throw new NotDivisibleByTwoException(size);
            _writer.CreateDatabase(ManagerUtilities.GetQualifiedName(name), size, 512);
        }

        public void ConnectDatabase(string name)
        {
            if (_connection) throw new SessionActiveException($"You already established a session to {name} database");

            _currentDatabase = ManagerUtilities.GetQualifiedName(name);
            _reader.ConnectDatabase(_currentDatabase);
            _connection = true;
        }

        public void DisconnectDatabase()
        {
            if (!_connection) throw new SessionNotCreatedException();

            Disk.Structures.Super = null;
            Disk.Structures.Inodes = null;
            Disk.Structures.BitMap = null;
            Disk.Structures.Directory = null;
            Disk.Structures = null;
            _connection = false;
        }

        public void DropDatabase(string name)
        {
            if (_connection) throw new SessionActiveException();
            if (!File.Exists(ManagerUtilities.GetQualifiedName(name))) throw new FileNotFoundException("No such database exists.");

            File.Delete(ManagerUtilities.GetQualifiedName(name));
        }

        public void CreateTable()
        {
            if (!_connection) throw new SessionNotCreatedException();

            var blocks = ManagerUtilities.GetBlocksFromBitmap(2);
            /*
             * 1) Extraer 2 bloques disponibles del bitmap. Uno para la metadata de la tabla y otro reservado para los registros.
             * 2) Recorrer la tabla de inodos en busca de uno libre. Inicializar sus campos con los respectivos valores.
             * 3) Ir al bloque de la metadata de la tabla y escribir los campos del modelo ColumnMetadata.
             * 4) Ir al directorio en busca de una entrada libre y marcarla como ocupada junto con el nombre de la tabla y el apuntador al inodo.
             */
        }
    }
}
