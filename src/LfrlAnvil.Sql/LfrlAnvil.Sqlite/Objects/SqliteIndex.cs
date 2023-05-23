using System;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteIndex : SqliteObject, ISqlIndex
{
    private readonly SqliteIndexColumn[] _columns;

    internal SqliteIndex(SqliteTable table, SqliteIndexBuilder builder)
        : base( builder )
    {
        Table = table;
        IsUnique = builder.IsUnique;

        var i = 0;
        var builderColumns = builder.Columns.Span;
        _columns = new SqliteIndexColumn[builderColumns.Length];
        foreach ( var c in builderColumns )
        {
            var column = table.Columns.Get( c.Column.Name );
            _columns[i++] = new SqliteIndexColumn( column, c.Ordering );
        }

        FullName = builder.FullName;
    }

    public SqliteTable Table { get; }
    public bool IsUnique { get; }
    public override string FullName { get; }
    public ReadOnlyMemory<SqliteIndexColumn> Columns => _columns;
    public override SqliteDatabase Database => Table.Database;

    ISqlTable ISqlIndex.Table => Table;
    ReadOnlyMemory<ISqlIndexColumn> ISqlIndex.Columns => _columns;
}
