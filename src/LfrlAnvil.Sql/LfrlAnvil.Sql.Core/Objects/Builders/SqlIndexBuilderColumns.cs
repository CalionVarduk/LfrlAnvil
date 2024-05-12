using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a collection of indexed SQL expression builders.
/// </summary>
/// <typeparam name="T">SQL column builder type.</typeparam>
public readonly struct SqlIndexBuilderColumns<T>
    where T : class, ISqlColumnBuilder
{
    /// <summary>
    /// Represents an empty collection.
    /// </summary>
    public static readonly SqlIndexBuilderColumns<T> Empty = new SqlIndexBuilderColumns<T>( ReadOnlyArray<SqlOrderByNode>.Empty );

    /// <summary>
    /// Creates a new <see cref="SqlIndexBuilderColumns{T}"/> instance.
    /// </summary>
    /// <param name="expressions">Underlying collection of expressions.</param>
    public SqlIndexBuilderColumns(ReadOnlyArray<SqlOrderByNode> expressions)
    {
        Expressions = expressions;
    }

    /// <summary>
    /// Underlying collection of expressions.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Expressions { get; }

    /// <summary>
    /// Attempts to return an SQL column builder instance at the specified 0-based position.
    /// </summary>
    /// <param name="index">0-based position.</param>
    /// <returns>SQL column builder instance or null when an expression at the specified position is not a single column.</returns>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? TryGet(int index)
    {
        return TryGetColumnFromNode( Expressions[index].Expression );
    }

    /// <summary>
    /// Checks whether or not an expression at the specified 0-based position is not a single column.
    /// </summary>
    /// <param name="index">0-based position.</param>
    /// <returns><b>true</b> when expression at the specified position is not a single column, otherwise <b>false</b>.</returns>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool IsExpression(int index)
    {
        return Expressions[index].Expression.NodeType != SqlNodeType.ColumnBuilder;
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Expressions );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="SqlIndexBuilderColumns{T}"/>.
    /// </summary>
    public struct Enumerator
    {
        private ReadOnlyArray<SqlOrderByNode>.Enumerator _enumerator;

        internal Enumerator(ReadOnlyArray<SqlOrderByNode> expressions)
        {
            _enumerator = expressions.GetEnumerator();
        }

        /// <summary>
        /// Gets the element at the current position of this enumerator.
        /// </summary>
        public T? Current => TryGetColumnFromNode( _enumerator.Current.Expression );

        /// <summary>
        /// Advances this enumerator to the next element.
        /// </summary>
        /// <returns><b>true</b> when next element exists, otherwise <b>false</b>.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T? TryGetColumnFromNode(SqlExpressionNode node)
    {
        return node.NodeType == SqlNodeType.ColumnBuilder
            ? ReinterpretCast.To<T>( ReinterpretCast.To<SqlColumnBuilderNode>( node ).Value )
            : null;
    }
}
