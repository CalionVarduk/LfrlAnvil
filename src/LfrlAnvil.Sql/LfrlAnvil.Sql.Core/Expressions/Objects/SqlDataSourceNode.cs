using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single data source.
/// </summary>
public abstract class SqlDataSourceNode : SqlNodeBase
{
    /// <summary>
    /// Creates a new <see cref="SqlDataSourceNode"/> instance.
    /// </summary>
    /// <param name="traits">Collection of decorating traits.</param>
    protected SqlDataSourceNode(Chain<SqlTraitNode> traits)
        : base( SqlNodeType.DataSource )
    {
        Traits = traits;
    }

    /// <summary>
    /// Collection of decorating traits.
    /// </summary>
    public Chain<SqlTraitNode> Traits { get; }

    /// <summary>
    /// First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.
    /// </summary>
    public abstract SqlRecordSetNode From { get; }

    /// <summary>
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </summary>
    public abstract ReadOnlyArray<SqlDataSourceJoinOnNode> Joins { get; }

    /// <summary>
    /// Collection of all record sets contained by this data source.
    /// </summary>
    public abstract IReadOnlyCollection<SqlRecordSetNode> RecordSets { get; }

    /// <summary>
    /// Gets a record set associated with this data source by its identifier.
    /// </summary>
    /// <param name="recordSetIdentifier">Record set's <see cref="SqlRecordSetNode.Identifier"/>.</param>
    /// <exception cref="KeyNotFoundException">When record set does not exist.</exception>
    public SqlRecordSetNode this[string recordSetIdentifier] => GetRecordSet( recordSetIdentifier );

    /// <summary>
    /// Returns a record set associated with this data source by its <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier">Record set's <see cref="SqlRecordSetNode.Identifier"/>.</param>
    /// <returns><see cref="SqlRecordSetNode"/> instance associated with the provided <paramref name="identifier"/>.</returns>
    /// <exception cref="KeyNotFoundException">When record set does not exist.</exception>
    [Pure]
    public abstract SqlRecordSetNode GetRecordSet(string identifier);

    /// <summary>
    /// Creates a new SQL data source syntax tree node by adding a new <paramref name="trait"/>.
    /// </summary>
    /// <param name="trait">Trait to add.</param>
    /// <returns>New SQL data source syntax tree node.</returns>
    [Pure]
    public virtual SqlDataSourceNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <summary>
    /// Creates a new SQL data source syntax tree node by changing the <see cref="Traits"/> collection.
    /// </summary>
    /// <param name="traits">Collection of traits to set.</param>
    /// <returns>New SQL data source syntax tree node.</returns>
    [Pure]
    public abstract SqlDataSourceNode SetTraits(Chain<SqlTraitNode> traits);
}
