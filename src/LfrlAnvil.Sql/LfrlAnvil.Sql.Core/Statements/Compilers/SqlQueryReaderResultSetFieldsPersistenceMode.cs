namespace LfrlAnvil.Sql.Statements.Compilers;

public enum SqlQueryReaderResultSetFieldsPersistenceMode : byte
{
    Ignore = 0,
    Persist = 1,
    PersistWithTypes = 2
}
