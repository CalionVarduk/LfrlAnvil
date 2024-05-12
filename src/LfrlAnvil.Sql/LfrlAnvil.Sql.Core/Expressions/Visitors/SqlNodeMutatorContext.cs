using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a context of an <see cref="ISqlNodeVisitor"/> that allows to replace parts of an SQL syntax tree.
/// </summary>
public class SqlNodeMutatorContext
{
    /// <summary>
    /// Visits the provided <paramref name="node"/> and returns the result of its mutation.
    /// </summary>
    /// <param name="node">Node to visit.</param>
    /// <returns>Result of <paramref name="node"/> mutation.</returns>
    [Pure]
    public SqlNodeBase Visit(SqlNodeBase node)
    {
        var mutator = new SqlNodeMutator( this );
        mutator.Visit( node );
        return mutator.GetResult();
    }

    /// <summary>
    /// Mutates the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to mutate.</param>
    /// <param name="ancestors"><see cref="SqlNodeAncestors"/> associated with the <paramref name="node"/> to mutate.</param>
    /// <returns>Mutation result.</returns>
    /// <remarks>
    /// See <see cref="Node(SqlNodeBase)"/> and <see cref="Leaf(SqlNodeBase)"/> for more information
    /// on how the result affects an SQL syntax tree traversal.
    /// </remarks>
    [Pure]
    protected internal virtual MutationResult Mutate(SqlNodeBase node, SqlNodeAncestors ancestors)
    {
        return Node( node );
    }

    /// <summary>
    /// Creates a new <see cref="MutationResult"/> instance with <see cref="MutationResult.IsNode"/> equal to <b>true</b>.
    /// </summary>
    /// <param name="value">SQL node that is the result of a mutation.</param>
    /// <returns>New <see cref="MutationResult"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static MutationResult Node(SqlNodeBase value)
    {
        return new MutationResult( value, isNode: true );
    }

    /// <summary>
    /// Creates a new <see cref="MutationResult"/> instance with <see cref="MutationResult.IsNode"/> equal to <b>false</b>.
    /// </summary>
    /// <param name="value">SQL node that is the result of a mutation.</param>
    /// <returns>New <see cref="MutationResult"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static MutationResult Leaf(SqlNodeBase value)
    {
        return new MutationResult( value, isNode: false );
    }

    /// <summary>
    /// Represents a result of an <see cref="SqlNodeBase"/> mutation attempt.
    /// </summary>
    protected internal readonly struct MutationResult
    {
        internal MutationResult(SqlNodeBase value, bool isNode)
        {
            Value = value;
            IsNode = isNode;
        }

        /// <summary>
        /// Underlying <see cref="SqlNodeBase"/> instance which is the result of mutation.
        /// </summary>
        public SqlNodeBase Value { get; }

        /// <summary>
        /// Specifies whether or not the <see cref="Value"/> should be recursively visited.
        /// </summary>
        /// <remarks>
        /// When this property is equal to <b>true</b>, then the <see cref="Value"/> is recursively visited,
        /// otherwise the recursion ends with the <see cref="Value"/> and its SQL syntax sub-tree will remain unchanged.
        /// </remarks>
        public bool IsNode { get; }

        /// <summary>
        /// Creates a new <see cref="MutationResult"/> instance with <see cref="IsNode"/> equal to <b>false</b>.
        /// </summary>
        /// <param name="value">SQL node.</param>
        /// <returns>New <see cref="MutationResult"/> instance.</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator MutationResult(SqlNodeBase value)
        {
            return Leaf( value );
        }

        /// <summary>
        /// Deconstruct the given <see cref="MutationResult"/> instance.
        /// </summary>
        /// <param name="value"><b>out</b> parameter that returns <see cref="Value"/>.</param>
        /// <param name="isNode"><b>out</b> parameter that returns <see cref="IsNode"/>.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Deconstruct(out SqlNodeBase value, out bool isNode)
        {
            value = Value;
            isNode = IsNode;
        }
    }
}
