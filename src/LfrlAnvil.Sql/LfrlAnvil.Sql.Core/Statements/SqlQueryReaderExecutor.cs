using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlQueryReaderExecutor(SqlQueryReader Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult Execute(IDbCommand command, SqlQueryReaderOptions? options = null)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader, options );
    }
}

public readonly record struct SqlQueryReaderExecutor<TRow>(SqlQueryReader<TRow> Reader, string Sql)
    where TRow : notnull
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult<TRow> Execute(IDbCommand command, SqlQueryReaderOptions? options = null)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader, options );
    }
}
