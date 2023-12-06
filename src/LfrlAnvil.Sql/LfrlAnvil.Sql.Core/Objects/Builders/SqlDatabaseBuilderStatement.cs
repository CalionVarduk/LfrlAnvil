using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlDatabaseBuilderStatement
{
    private SqlDatabaseBuilderStatement(SqlNodeInterpreterContext context, Action<IDbCommand>? beforeCallback)
    {
        Sql = context.Sql.AppendLine().ToString();
        BeforeCallback = beforeCallback;
    }

    public string Sql { get; }
    public Action<IDbCommand>? BeforeCallback { get; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Apply(IDbCommand command)
    {
        command.CommandText = Sql;
        BeforeCallback?.Invoke( command );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseBuilderStatement Create(SqlNodeInterpreterContext context)
    {
        return new SqlDatabaseBuilderStatement( context, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseBuilderStatement Create(SqlNodeInterpreterContext context, SqlParameterBinderExecutor parameters)
    {
        return new SqlDatabaseBuilderStatement( context, parameters.Execute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseBuilderStatement Create<TSource>(
        SqlNodeInterpreterContext context,
        SqlParameterBinderExecutor<TSource> parameters)
        where TSource : notnull
    {
        return new SqlDatabaseBuilderStatement( context, parameters.Execute );
    }
}
