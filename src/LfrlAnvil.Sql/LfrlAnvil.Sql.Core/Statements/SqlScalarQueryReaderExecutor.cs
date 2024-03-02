using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlScalarQueryReaderExecutor(SqlScalarQueryReader Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult Execute(IDbCommand command)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader );
    }
}

public readonly record struct SqlScalarQueryReaderExecutor<T>(SqlScalarQueryReader<T> Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult<T> Execute(IDbCommand command)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader );
    }
}
