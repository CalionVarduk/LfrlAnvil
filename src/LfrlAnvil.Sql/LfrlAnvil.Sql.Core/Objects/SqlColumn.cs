using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlColumn" />
public abstract class SqlColumn : SqlObject, ISqlColumn
{
    private SqlColumnNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlColumn"/> instance.
    /// </summary>
    /// <param name="table">Table that this column belongs to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlColumn(SqlTable table, SqlColumnBuilder builder)
        : base( table.Database, builder )
    {
        Table = table;
        IsNullable = builder.IsNullable;
        HasDefaultValue = builder.DefaultValue is not null;
        ComputationStorage = builder.Computation?.Storage;
        TypeDefinition = builder.TypeDefinition;
        _node = null;
    }

    /// <inheritdoc cref="ISqlColumn.Table" />
    public SqlTable Table { get; }

    /// <inheritdoc />
    public bool IsNullable { get; }

    /// <inheritdoc />
    public bool HasDefaultValue { get; }

    /// <inheritdoc />
    public SqlColumnComputationStorage? ComputationStorage { get; }

    /// <inheritdoc cref="ISqlColumn.TypeDefinition" />
    public SqlColumnTypeDefinition TypeDefinition { get; }

    /// <inheritdoc />
    public SqlColumnNode Node => _node ??= Table.Node[Name];

    ISqlTable ISqlColumn.Table => Table;
    ISqlColumnTypeDefinition ISqlColumn.TypeDefinition => TypeDefinition;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlColumn"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    /// <inheritdoc />
    [Pure]
    public SqlOrderByNode Asc()
    {
        return SqlNode.OrderByAsc( Node );
    }

    /// <inheritdoc />
    [Pure]
    public SqlOrderByNode Desc()
    {
        return SqlNode.OrderByDesc( Node );
    }
}
