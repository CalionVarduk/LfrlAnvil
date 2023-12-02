using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlQueryReaderResult
{
    public static readonly SqlQueryReaderResult Empty = default;

    private readonly SqlResultSetField[]? _resultSetFields;

    public SqlQueryReaderResult(SqlResultSetField[] resultSetFields, List<object?> cells)
    {
        _resultSetFields = resultSetFields;
        Rows = cells.Count == 0 ? null : new SqlQueryReaderRowCollection( _resultSetFields, cells );
    }

    public SqlQueryReaderRowCollection? Rows { get; }
    public ReadOnlySpan<SqlResultSetField> ResultSetFields => _resultSetFields;

    [MemberNotNullWhen( false, nameof( Rows ) )]
    public bool IsEmpty => Rows is null;
}

public readonly struct SqlQueryReaderResult<TRow>
    where TRow : notnull
{
    public static readonly SqlQueryReaderResult<TRow> Empty = default;

    private readonly SqlResultSetField[]? _resultSetFields;

    public SqlQueryReaderResult(SqlResultSetField[]? resultSetFields, List<TRow> rows)
    {
        _resultSetFields = resultSetFields;
        Rows = rows.Count == 0 ? null : rows;
    }

    public List<TRow>? Rows { get; }
    public ReadOnlySpan<SqlResultSetField> ResultSetFields => _resultSetFields;

    [MemberNotNullWhen( false, nameof( Rows ) )]
    public bool IsEmpty => Rows is null;
}
