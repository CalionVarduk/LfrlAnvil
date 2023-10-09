using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal readonly record struct SqlQueryDefinition<T>(string Sql, Func<SqliteCommand, T> Executor)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Execute(SqliteCommand command)
    {
        command.CommandText = Sql;
        return Executor( command );
    }
}
