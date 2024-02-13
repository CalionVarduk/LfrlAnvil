using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql;

public static class SqlObjectExtensions
{
    public static int GetPendingActionCount(this ISqlDatabaseBuilder database)
    {
        return database.Changes.GetPendingActions().Length;
    }

    public static SqlDatabaseBuilderCommandAction[] GetLastPendingActions(this ISqlDatabaseBuilder database, int skip)
    {
        return database.Changes.GetPendingActions().Slice( skip ).ToArray();
    }
}
