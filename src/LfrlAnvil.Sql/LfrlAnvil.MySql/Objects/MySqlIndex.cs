using System.Collections.Generic;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlIndex : MySqlConstraint, ISqlIndex
{
    private readonly SqlIndexColumn<ISqlColumn>[] _columns;

    internal MySqlIndex(MySqlTable table, MySqlIndexBuilder builder)
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
    public override MySqlDatabase Database => Table.Database;

    IReadOnlyList<SqlIndexColumn<ISqlColumn>> ISqlIndex.Columns => _columns;
}
