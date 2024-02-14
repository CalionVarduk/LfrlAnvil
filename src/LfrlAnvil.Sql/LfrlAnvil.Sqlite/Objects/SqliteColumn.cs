using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteColumn : SqliteObject, ISqlColumn
{
    private SqlColumnNode? _node;

    internal SqliteColumn(SqliteTable table, SqliteColumnBuilder builder)
        : base( builder )
    {
        Table = table;
        TypeDefinition = builder.TypeDefinition;
        IsNullable = builder.IsNullable;
        HasDefaultValue = builder.DefaultValue is not null;
        _node = null;
    }

    public SqliteTable Table { get; }
    public SqliteColumnTypeDefinition TypeDefinition { get; }
    public bool IsNullable { get; }
    public bool HasDefaultValue { get; }
    public SqlColumnNode Node => _node ??= Table.Node[Name];
    public override SqliteDatabase Database => Table.Schema.Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    [Pure]
    public SqlIndexColumn<SqliteColumn> Asc()
    {
        return SqlIndexColumn.CreateAsc( this );
    }

    [Pure]
    public SqlIndexColumn<SqliteColumn> Desc()
    {
        return SqlIndexColumn.CreateDesc( this );
    }

    ISqlTable ISqlColumn.Table => Table;
    ISqlColumnTypeDefinition ISqlColumn.TypeDefinition => TypeDefinition;

    [Pure]
    SqlIndexColumn<ISqlColumn> ISqlColumn.Asc()
    {
        return Asc();
    }

    [Pure]
    SqlIndexColumn<ISqlColumn> ISqlColumn.Desc()
    {
        return Desc();
    }
}
