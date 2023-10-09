namespace LfrlAnvil.Sql.Events;

public enum SqlDatabaseFactoryStatementType : byte
{
    Change = 0,
    VersionHistory = 1,
    Other = 2
}
