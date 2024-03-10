using System.Collections.Generic;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlIndex : SqlConstraint, ISqlIndex
{
    private readonly ReadOnlyArray<SqlIndexed<ISqlColumn>> _columns;

    protected SqlIndex(SqlTable table, SqlIndexBuilder builder)
        : base( table, builder )
    {
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;
        IsVirtual = builder.IsVirtual;

        var columns = new SqlIndexed<ISqlColumn>[builder.Columns.Expressions.Count];
        for ( var i = 0; i < columns.Length; ++i )
        {
            var column = builder.Columns.TryGet( i );
            columns[i] = new SqlIndexed<ISqlColumn>(
                column is null ? null : table.Columns.Get( column.Name ),
                builder.Columns.Expressions[i].Ordering );
        }

        _columns = columns;
    }

    public bool IsUnique { get; }
    public bool IsPartial { get; }
    public bool IsVirtual { get; }
    public SqlIndexedArray<SqlColumn> Columns => SqlIndexedArray<SqlColumn>.From( _columns );

    IReadOnlyList<SqlIndexed<ISqlColumn>> ISqlIndex.Columns => _columns.GetUnderlyingArray();
}
