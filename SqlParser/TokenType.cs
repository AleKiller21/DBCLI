﻿namespace SqlParser
{
    public enum TokenType
    {
        Id,
        LiteralString,
        LiteralInt,
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
        RwAnd,
        RwOr,
        RwMb,
        RwGb,
        OpAll,
        OpEqual,
        OpGreaterThan,
        OpLessThan,
        OpGreaterThanOrEqual,
        OpLessThanOrEqual,
        ParenthesisOpen,
        ParenthesisClose,
        EndStatement,
        Eof
    }
}
