using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set.
/// </summary>
public abstract class SqlRecordSetNode : SqlNodeBase
{
    internal SqlRecordSetNode(SqlNodeType nodeType, string? alias, bool isOptional)
        : base( nodeType )
    {
        IsOptional = isOptional;
        Alias = alias;
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecordSetNode"/> with <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="isOptional"></param>
    protected SqlRecordSetNode(string? alias, bool isOptional)
    {
        IsOptional = isOptional;
        Alias = alias;
    }

    /// <summary>
    /// Specifies whether or not this record set is marked as optional.
    /// </summary>
    /// <remarks>Optional record sets will only contain nullable data fields.</remarks>
    public bool IsOptional { get; }

    /// <summary>
    /// Optional alias of this record set.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// <see cref="SqlRecordSetInfo"/> associated with this record set.
    /// </summary>
    public abstract SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Specifies whether or not this record set is aliased.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Alias ) )]
    public bool IsAliased => Alias is not null;

    /// <summary>
    /// Gets the internal identifier of this record set. This can be used to get a record set from a <see cref="SqlMultiDataSourceNode"/>.
    /// </summary>
    public string Identifier => Alias ?? Info.Identifier;

    /// <summary>
    /// Gets a data field associated with this record set by its name.
    /// </summary>
    /// <param name="fieldName">Name of the data field to get.</param>
    /// <exception cref="KeyNotFoundException">When data field does not exist.</exception>
    public SqlDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <summary>
    /// Returns a collection of all known data fields that belong to this record set.
    /// </summary>
    /// <returns>Collection of all known data fields that belong to this record set.</returns>
    [Pure]
    public abstract IReadOnlyCollection<SqlDataFieldNode> GetKnownFields();

    /// <summary>
    /// Returns an unsafe data field associated with this record set by its <paramref name="name"/>.
    /// If a known data field by the provided <paramref name="name"/> does not exist, then a new <see cref="SqlRawDataFieldNode"/> instance
    /// will be returned instead.
    /// </summary>
    /// <param name="name">Data field's name.</param>
    /// <returns><see cref="SqlDataFieldNode"/> instance associated with the provided <paramref name="name"/>.</returns>
    [Pure]
    public abstract SqlDataFieldNode GetUnsafeField(string name);

    /// <summary>
    /// Returns a data field associated with this record set by its <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Data field's name.</param>
    /// <returns><see cref="SqlDataFieldNode"/> instance associated with the provided <paramref name="name"/>.</returns>
    /// <exception cref="KeyNotFoundException">When data field does not exist.</exception>
    [Pure]
    public abstract SqlDataFieldNode GetField(string name);

    /// <summary>
    /// Creates a new SQL record set node with changed <see cref="Alias"/>.
    /// </summary>
    /// <param name="alias">Alias to set.</param>
    /// <returns>New SQL record set node.</returns>
    [Pure]
    public abstract SqlRecordSetNode As(string alias);

    /// <summary>
    /// Creates a new SQL record set node without an alias.
    /// </summary>
    /// <returns>New SQL record set node.</returns>
    [Pure]
    public abstract SqlRecordSetNode AsSelf();

    /// <summary>
    /// Creates a new SQL record set node with changed <see cref="IsOptional"/>.
    /// </summary>
    /// <param name="optional"><see cref="IsOptional"/> value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New SQL record set node.</returns>
    [Pure]
    public abstract SqlRecordSetNode MarkAsOptional(bool optional = true);
}
