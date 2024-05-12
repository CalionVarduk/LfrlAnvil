using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single data field of a record set
/// based on an <see cref="ISqlViewDataField"/> instance.
/// </summary>
public sealed class SqlViewDataFieldNode : SqlDataFieldNode
{
    internal SqlViewDataFieldNode(SqlRecordSetNode recordSet, ISqlViewDataField value)
        : base( recordSet, SqlNodeType.ViewDataField )
    {
        Value = value;
    }

    /// <summary>
    /// Underlying <see cref="ISqlViewDataField"/> instance.
    /// </summary>
    public ISqlViewDataField Value { get; }

    /// <inheritdoc />
    public override string Name => Value.Name;

    /// <inheritdoc />
    [Pure]
    public override SqlViewDataFieldNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlViewDataFieldNode( recordSet, Value );
    }
}
