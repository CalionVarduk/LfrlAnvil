using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single data field of a record set based on a raw SQL name.
/// </summary>
public sealed class SqlRawDataFieldNode : SqlDataFieldNode
{
    internal SqlRawDataFieldNode(SqlRecordSetNode recordSet, string name, TypeNullability? type)
        : base( recordSet, SqlNodeType.RawDataField )
    {
        Name = name;
        Type = type;
    }

    /// <inheritdoc />
    public override string Name { get; }

    /// <summary>
    /// Optional runtime type of this data field.
    /// </summary>
    public TypeNullability? Type { get; }

    /// <inheritdoc />
    [Pure]
    public override SqlRawDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlRawDataFieldNode( recordSet, Name, Type );
    }
}
