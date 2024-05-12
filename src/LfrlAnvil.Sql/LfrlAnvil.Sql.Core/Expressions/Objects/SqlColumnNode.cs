using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single data field of a record set
/// based on an <see cref="ISqlColumn"/> instance.
/// </summary>
public sealed class SqlColumnNode : SqlDataFieldNode
{
    internal SqlColumnNode(SqlRecordSetNode recordSet, ISqlColumn value, bool isOptional)
        : base( recordSet, SqlNodeType.Column )
    {
        Value = value;
        Type = TypeNullability.Create( Value.TypeDefinition.RuntimeType, isOptional || Value.IsNullable );
    }

    /// <summary>
    /// Underlying <see cref="ISqlColumn"/> instance.
    /// </summary>
    public ISqlColumn Value { get; }

    /// <summary>
    /// Runtime type of this data field.
    /// </summary>
    public TypeNullability Type { get; }

    /// <inheritdoc />
    public override string Name => Value.Name;

    /// <inheritdoc />
    [Pure]
    public override SqlColumnNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlColumnNode( recordSet, Value, Type.IsNullable );
    }
}
