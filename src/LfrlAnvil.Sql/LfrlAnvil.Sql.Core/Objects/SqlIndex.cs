using System.Collections.Generic;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlIndex : SqlConstraint, ISqlIndex
{
    private readonly ReadOnlyArray<SqlIndexColumn<ISqlColumn>> _columns;

    protected SqlIndex(SqlTable table, SqlIndexBuilder builder)
        : base( table, builder )
    {
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;

        var columns = new SqlIndexColumn<ISqlColumn>[builder.Columns.Count];
        for ( var i = 0; i < columns.Length; ++i )
        {
            var c = builder.Columns[i];
            columns[i] = SqlIndexColumn.Create( table.Columns.Get( c.Column.Name ), c.Ordering );
        }

        _columns = columns;
    }

    public bool IsUnique { get; }
    public bool IsPartial { get; }
    public SqlIndexColumnArray<SqlColumn> Columns => SqlIndexColumnArray<SqlColumn>.From( _columns );

    IReadOnlyList<SqlIndexColumn<ISqlColumn>> ISqlIndex.Columns => _columns.GetUnderlyingArray();
}
