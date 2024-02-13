using System;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlIndex : MySqlConstraint, ISqlIndex
{
    private readonly MySqlIndexColumn[] _columns;

    internal MySqlIndex(MySqlTable table, MySqlIndexBuilder builder)
        : base( table, builder )
    {
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;

        var i = 0;
        var builderColumns = builder.Columns;
        _columns = new MySqlIndexColumn[builderColumns.Count];
        foreach ( var c in builderColumns )
        {
            var column = table.Columns.GetColumn( c.Column.Name );
            _columns[i++] = new MySqlIndexColumn( column, c.Ordering );
        }
    }

    public bool IsUnique { get; }
    public bool IsPartial { get; }
    public ReadOnlyMemory<MySqlIndexColumn> Columns => _columns;
    public override MySqlDatabase Database => Table.Database;

    ReadOnlyMemory<ISqlIndexColumn> ISqlIndex.Columns => _columns;
}
