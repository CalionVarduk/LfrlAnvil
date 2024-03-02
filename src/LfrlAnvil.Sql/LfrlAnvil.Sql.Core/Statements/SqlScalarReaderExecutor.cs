using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlScalarReaderExecutor(SqlScalarReader Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarResult Execute(IDbCommand command)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader );
    }
}

public readonly record struct SqlScalarReaderExecutor<T>(SqlScalarReader<T> Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarResult<T> Execute(IDbCommand command)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader );
    }
}
