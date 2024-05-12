using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a primary key constraint.
/// </summary>
public sealed class SqlPrimaryKeyDefinitionNode : SqlNodeBase
{
    internal SqlPrimaryKeyDefinitionNode(SqlSchemaObjectName name, ReadOnlyArray<SqlOrderByNode> columns)
        : base( SqlNodeType.PrimaryKeyDefinition )
    {
        Name = name;
        Columns = columns;
    }

    /// <summary>
    /// Primary key constraint's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Collection of columns that define this primary key constraint.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Columns { get; }
}
