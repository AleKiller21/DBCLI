﻿namespace SqlParser
{
    public enum TokenType
    {
        Id,
        LiteralString,
        LiteralInt,
        LiteralDouble,
        RwConnect,
        RwDisconnect,
        RwCreate,
        RwDrop,
        RwInsert,
        RwSelect,
        RwUpdate,
        RwDelete,
        RwInto,
        RwValues,
        RwFrom,
        RwSet,
        RwWhere,
        RwDatabase,
        RwTable,
        RwAnd,
        RwOr,
        RwMb,
        RwGb,
        RwInt,
        RwChar,
        RwDouble,
        RwShow,
        RwAllTables,
        OpAll,
        OpEqual,
        OpNotEqual,
        OpGreaterThan,
        OpLessThan,
        OpGreaterThanOrEqual,
        OpLessThanOrEqual,
        ParenthesisOpen,
        ParenthesisClose,
        Comma,
        EndStatement,
        Eof
    }
}
