using System;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlIndex : MySqlObject, ISqlIndex
{
    private readonly MySqlIndexColumn[] _columns;

    internal MySqlIndex(MySqlTable table, MySqlIndexBuilder builder)
        : base( builder )
    {
        Table = table;
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;
        FullName = builder.FullName;

        var i = 0;
        var builderColumns = builder.Columns.Span;
        _columns = new MySqlIndexColumn[builderColumns.Length];
        foreach ( var c in builderColumns )
        {
            var column = table.Columns.Get( c.Column.Name );
            _columns[i++] = new MySqlIndexColumn( column, c.Ordering );
        }
    }

    public MySqlTable Table { get; }
    public bool IsUnique { get; }
    public bool IsPartial { get; }
    public override string FullName { get; }
    public ReadOnlyMemory<MySqlIndexColumn> Columns => _columns;
    public override MySqlDatabase Database => Table.Database;

    ISqlTable ISqlIndex.Table => Table;
    ReadOnlyMemory<ISqlIndexColumn> ISqlIndex.Columns => _columns;
}
