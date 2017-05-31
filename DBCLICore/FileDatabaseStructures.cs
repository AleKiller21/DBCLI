using DBCLICore.Models;

namespace DBCLICore
{
    public class FileDatabaseStructures
    {
        public SuperBlock Super;
        public DirectoryEntry[] Directory;
        public Inode[] Inodes;
    }
}
