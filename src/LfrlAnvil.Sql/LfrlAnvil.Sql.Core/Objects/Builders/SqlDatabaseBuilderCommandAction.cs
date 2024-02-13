using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlDatabaseBuilderCommandAction
{
    private SqlDatabaseBuilderCommandAction(string? sql, Action<IDbCommand>? callback)
    {
        Sql = sql;
        Callback = callback;
    }

    public string? Sql { get; }
    public Action<IDbCommand>? Callback { get; }

    [Pure]
    public override string ToString()
    {
        if ( Sql is null )
            return "<Callback>";

        var header = Callback is null ? "<Sql>" : "<Sql> <Parameterized>";
        return $"{header}{Environment.NewLine}{Environment.NewLine}{Sql}";
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql(string sql)
    {
        return new SqlDatabaseBuilderCommandAction( sql, callback: null );
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql(string sql, SqlParameterBinderExecutor boundParameters)
    {
        return new SqlDatabaseBuilderCommandAction( sql, boundParameters.Execute );
    }

    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql<T>(string sql, SqlParameterBinderExecutor<T> boundParameters)
        where T : notnull
    {
        return new SqlDatabaseBuilderCommandAction( sql, boundParameters.Execute );
    }

    // TODO:
    // callback may require additional work, since it would be good to have a logged execution from db factory level
    // also, right now loops responsible for statement application assume that command text is set & then it gets ran
    // there is no way to e.g. run statements conditionally, which would be the main point of these custom actions
    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateCallback(Action<IDbCommand> callback)
    {
        return new SqlDatabaseBuilderCommandAction( sql: null, callback );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Apply(IDbCommand command)
    {
        if ( Sql is not null )
            command.CommandText = Sql;

        if ( Callback is not null )
            Callback.Invoke( command );
        else
            command.Parameters.Clear();
    }
}
