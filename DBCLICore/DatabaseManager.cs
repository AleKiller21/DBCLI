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
        private const string Path = "..\\Databases\\";
        private string _currentDatabase;
        private bool _connection;
        private readonly FileDatabaseWriter _writer;
        private readonly FileDatabaseReader _reader;
        private FileDatabaseStructures _structures;

        public DatabaseManager()
        {
            _connection = false;
            _writer = new FileDatabaseWriter();
            _reader = new FileDatabaseReader();
        }

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ConvertToBytes(size, unit);

            if (!CheckIfSizeDivisibleByTwo(size)) throw new NotDivisibleByTwoException(size);
            _writer.CreateDatabase(GetQualifiedName(name), size, 512);
        }

        public void ConnectDatabase(string name)
        {
            if (_connection) throw new SessionActiveException($"You already established a session to {name} database");

            _currentDatabase = GetQualifiedName(name);
            _structures = _reader.ConnectDatabase(_currentDatabase);
            _connection = true;
        }

        public void DisconnectDatabase()
        {
            if (!_connection) throw new SessionNotCreatedException();

            _structures.Super = null;
            _structures.Inodes = null;
            _structures.BitMap = null;
            _structures.Directory = null;
            _connection = false;
        }

        public void DropDatabase(string name)
        {
            if (_connection) throw new SessionActiveException();
            if (!File.Exists(GetQualifiedName(name))) throw new FileNotFoundException("No such database exists.");

            File.Delete(GetQualifiedName(name));
        }

        public void CreateTable()
        {
            if (!_connection) throw new SessionNotCreatedException();

            var blocks = GetBlocksFromBitmap(2);
            /*
             * 1) Extraer 2 bloques disponibles del bitmap. Uno para la metadata de la tabla y otro reservado para los registros.
             * 2) Recorrer la tabla de inodos en busca de uno libre. Inicializar sus campos con los respectivos valores.
             * 3) Ir al bloque de la metadata de la tabla y escribir los campos del modelo ColumnMetadata.
             * 4) Ir al directorio en busca de una entrada libre y marcarla como ocupada junto con el nombre de la tabla y el apuntador al inodo.
             */
        }

        private List<int> GetBlocksFromBitmap(int amount)
        {
            var wordsOccupied = 0;
            var extractedBlocks = new List<int>();
            const int msb = 128;

            for (var i = 0; i < _structures.BitMap.Length; i++)
            {
                if (_structures.BitMap[i] == 0)
                {
                    wordsOccupied++;
                    continue;
                }

                for (var bit = 0; bit < sizeof(byte) * 8; bit++)
                {
                    var block = _structures.BitMap[i] & (msb >> bit);
                    if(block == 0) continue;

                    extractedBlocks.Add(sizeof(byte)*8 * wordsOccupied + bit);
                    _structures.BitMap[i] ^= (byte)(msb >> bit);
                    if (extractedBlocks.Count == amount) break;
                }

                if (extractedBlocks.Count == amount) break;
            }

            if (extractedBlocks.Count == 0) throw new NotEnoughFreeBlocksException();

            _structures.Super.FreeBlocks -= amount;
            _structures.Super.UsedBlocks += amount;

            return extractedBlocks;
        }

        private long ConvertToBytes(long size, UnitSize unit)
        {
            size *= 1024;
            if (unit == UnitSize.Mb) return size * 1024;
            return size * 1024 * 1024;
        }

        private bool CheckIfSizeDivisibleByTwo(long size)
        {
            return size % 2 == 0;
        }

        private string GetQualifiedName(string name)
        {
            return Path + name + ".db";
        }
    }
}
