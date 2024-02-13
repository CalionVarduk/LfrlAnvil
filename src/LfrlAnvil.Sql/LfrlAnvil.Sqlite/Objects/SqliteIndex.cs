using System;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteIndex : SqliteConstraint, ISqlIndex
{
    private readonly SqliteIndexColumn[] _columns;

    internal SqliteIndex(SqliteTable table, SqliteIndexBuilder builder)
        : base( table, builder )
    {
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;

        var i = 0;
        var builderColumns = builder.Columns;
        _columns = new SqliteIndexColumn[builderColumns.Count];
        foreach ( var c in builderColumns )
        {
            var column = table.Columns.GetColumn( c.Column.Name );
            _columns[i++] = new SqliteIndexColumn( column, c.Ordering );
        }
    }

    public bool IsUnique { get; }
    public bool IsPartial { get; }
    public ReadOnlyMemory<SqliteIndexColumn> Columns => _columns;
    public override SqliteDatabase Database => Table.Database;

    ReadOnlyMemory<ISqlIndexColumn> ISqlIndex.Columns => _columns;
}
