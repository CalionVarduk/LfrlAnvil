using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlColumn : SqlObject, ISqlColumn
{
    private SqlColumnNode? _node;

    protected SqlColumn(SqlTable table, SqlColumnBuilder builder)
        : base( table.Database, builder )
    {
        Table = table;
        IsNullable = builder.IsNullable;
        HasDefaultValue = builder.DefaultValue is not null;
        TypeDefinition = builder.TypeDefinition;
        _node = null;
    }

    public SqlTable Table { get; }
    public bool IsNullable { get; }
    public bool HasDefaultValue { get; }
    public SqlColumnTypeDefinition TypeDefinition { get; }
    public SqlColumnNode Node => _node ??= Table.Node[Name];

    ISqlTable ISqlColumn.Table => Table;
    ISqlColumnTypeDefinition ISqlColumn.TypeDefinition => TypeDefinition;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    [Pure]
    public SqlIndexColumn<SqlColumn> Asc()
    {
        return SqlIndexColumn.CreateAsc( this );
    }

    [Pure]
    public SqlIndexColumn<SqlColumn> Desc()
    {
        return SqlIndexColumn.CreateDesc( this );
    }

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
