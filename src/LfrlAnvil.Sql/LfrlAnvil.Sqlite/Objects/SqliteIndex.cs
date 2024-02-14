using System.Collections.Generic;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteIndex : SqliteConstraint, ISqlIndex
{
    private readonly SqlIndexColumn<ISqlColumn>[] _columns;

    internal SqliteIndex(SqliteTable table, SqliteIndexBuilder builder)
        : base( table, builder )
    {
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;

        var i = 0;
        var builderColumns = builder.Columns;
        _columns = new SqlIndexColumn<ISqlColumn>[builderColumns.Count];
        foreach ( var c in builderColumns )
        {
            var column = table.Columns.Get( c.Column.Name );
            _columns[i++] = SqlIndexColumn.Create( column, c.Ordering );
        }
    }

    public bool IsUnique { get; }
    public bool IsPartial { get; }
    public ReadOnlyArray<SqlIndexColumn<ISqlColumn>> Columns => _columns;
    public override SqliteDatabase Database => Table.Database;

    IReadOnlyList<SqlIndexColumn<ISqlColumn>> ISqlIndex.Columns => _columns;
}
