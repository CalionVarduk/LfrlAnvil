using System;

namespace LfrlAnvil.Sql.Events;

public interface ISqlDatabaseFactoryStatementListener
{
    void OnBefore(SqlDatabaseFactoryStatementEvent @event);
    void OnAfter(SqlDatabaseFactoryStatementEvent @event, TimeSpan elapsedTime, Exception? exception);
}
