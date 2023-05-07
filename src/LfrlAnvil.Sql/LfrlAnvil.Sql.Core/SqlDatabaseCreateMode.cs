namespace LfrlAnvil.Sql;

public enum SqlDatabaseCreateMode : byte
{
    NoChanges = 0,
    DryRun = 1,
    Commit = 2
}
