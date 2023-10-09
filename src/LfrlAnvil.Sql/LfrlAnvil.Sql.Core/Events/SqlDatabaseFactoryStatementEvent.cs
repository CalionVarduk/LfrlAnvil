using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Events;

public readonly record struct SqlDatabaseFactoryStatementEvent(
    SqlDatabaseFactoryStatementKey Key,
    string Sql,
    IReadOnlyList<KeyValuePair<string, object?>> Parameters,
    SqlDatabaseFactoryStatementType Type,
    DateTime UtcStartDate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseFactoryStatementEvent Create(
        SqlDatabaseFactoryStatementKey key,
        DbCommand command,
        SqlDatabaseFactoryStatementType type)
    {
        var parameters = GetCommandParameters( command );
        return new SqlDatabaseFactoryStatementEvent( key, command.CommandText, parameters, type, DateTime.UtcNow );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static KeyValuePair<string, object?>[] GetCommandParameters(DbCommand command)
    {
        var commandParameters = command.Parameters;
        var commandParameterCount = commandParameters.Count;
        if ( commandParameterCount == 0 )
            return Array.Empty<KeyValuePair<string, object?>>();

        var result = new KeyValuePair<string, object?>[commandParameterCount];
        for ( var i = 0; i < commandParameterCount; ++i )
        {
            var parameter = commandParameters[i];
            result[i] = KeyValuePair.Create( parameter.ParameterName, parameter.Value );
        }

        return result;
    }
}
