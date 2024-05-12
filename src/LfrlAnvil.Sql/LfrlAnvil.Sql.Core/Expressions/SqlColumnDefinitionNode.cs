using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a table column.
/// </summary>
public class SqlColumnDefinitionNode : SqlNodeBase
{
    internal SqlColumnDefinitionNode(
        string name,
        TypeNullability type,
        SqlExpressionNode? defaultValue,
        SqlColumnComputation? computation)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
        TypeDefinition = null;
        DefaultValue = defaultValue;
        Computation = computation;
    }

    internal SqlColumnDefinitionNode(
        string name,
        ISqlColumnTypeDefinition typeDefinition,
        bool isNullable,
        SqlExpressionNode? defaultValue,
        SqlColumnComputation? computation)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = TypeNullability.Create( typeDefinition.RuntimeType, isNullable );
        TypeDefinition = typeDefinition;
        DefaultValue = defaultValue;
        Computation = computation;
    }

    /// <summary>
    /// Column's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Column's runtime type.
    /// </summary>
    public TypeNullability Type { get; }

    /// <summary>
    /// Optional <see cref="ISqlColumnTypeDefinition"/> instance that defines this column's type.
    /// </summary>
    public ISqlColumnTypeDefinition? TypeDefinition { get; }

    /// <summary>
    /// Column's optional default value.
    /// </summary>
    public SqlExpressionNode? DefaultValue { get; }

    /// <summary>
    /// Column's optional computation.
    /// </summary>
    public SqlColumnComputation? Computation { get; }
}
