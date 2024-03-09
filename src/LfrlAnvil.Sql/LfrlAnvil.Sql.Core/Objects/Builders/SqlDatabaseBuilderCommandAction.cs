using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlDatabaseBuilderCommandAction
{
    private static readonly Func<IDbCommand, object?> DefaultOnExecute = static cmd =>
    {
        cmd.ExecuteNonQuery();
        return null;
    };

    private SqlDatabaseBuilderCommandAction(
        string? sql,
        Action<IDbCommand>? onCommandSetup,
        Func<IDbCommand, object?> onExecute,
        TimeSpan? timeout)
    {
        Sql = sql;
        OnCommandSetup = onCommandSetup;
        OnExecute = onExecute;
        Timeout = timeout;
    }

    public string? Sql { get; }
    public Action<IDbCommand>? OnCommandSetup { get; }
    public Func<IDbCommand, object?> OnExecute { get; }
    public TimeSpan? Timeout { get; }

    [Pure]
    public override string ToString()
    {
        if ( Sql is null )
            return OnCommandSetup is null ? "<Custom>" : "<Custom> <WithSetup>";

        var header = OnCommandSetup is null ? "<Sql>" : "<Sql> <WithSetup>";
        return $"{header}{Environment.NewLine}{Sql}";
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql(string sql, TimeSpan? timeout = null)
    {
        return new SqlDatabaseBuilderCommandAction( sql, onCommandSetup: null, DefaultOnExecute, timeout );
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql(
        string sql,
        SqlParameterBinderExecutor boundParameters,
        TimeSpan? timeout = null)
    {
        return new SqlDatabaseBuilderCommandAction( sql, boundParameters.Execute, DefaultOnExecute, timeout );
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql<T>(
        string sql,
        SqlParameterBinderExecutor<T> boundParameters,
        TimeSpan? timeout = null)
        where T : notnull
    {
        return new SqlDatabaseBuilderCommandAction( sql, boundParameters.Execute, DefaultOnExecute, timeout );
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateCustom(
        Action<IDbCommand> onExecute,
        Action<IDbCommand>? onCommandSetup = null,
        TimeSpan? timeout = null)
    {
        return new SqlDatabaseBuilderCommandAction(
            sql: null,
            onCommandSetup,
            cmd =>
            {
                onExecute( cmd );
                return null;
            },
            timeout );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void PrepareCommand(IDbCommand command)
    {
        if ( Sql is not null )
            command.CommandText = Sql;

        if ( OnCommandSetup is not null )
            OnCommandSetup.Invoke( command );
        else
            command.Parameters.Clear();
    }
}
