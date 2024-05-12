using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a stack of <see cref="SqlNodeBase"/> ancestor nodes.
/// </summary>
public readonly struct SqlNodeAncestors
{
    private readonly List<SqlNodeBase> _ancestors;

    internal SqlNodeAncestors(List<SqlNodeBase> ancestors)
    {
        _ancestors = ancestors;
    }

    /// <summary>
    /// Number of ancestors.
    /// </summary>
    public int Count => _ancestors.Count;

    /// <summary>
    /// Gets an ancestor node at the provided 0-base <paramref name="index"/> in this stack.
    /// </summary>
    /// <param name="index">0-based position of the ancestor node to get.</param>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public SqlNodeBase this[int index] => _ancestors[_ancestors.Count - index - 1];

    /// <summary>
    /// Attempts to find a 0-based index of the provided <paramref name="node"/> in this stack.
    /// </summary>
    /// <param name="node">Ancestor node to find.</param>
    /// <returns>0-based index of the provided <paramref name="node"/> if it exists in this stack, otherwise <b>-1</b>.</returns>
    [Pure]
    public int FindIndex(SqlNodeBase node)
    {
        for ( var i = _ancestors.Count - 1; i >= 0; --i )
        {
            if ( ReferenceEquals( _ancestors[i], node ) )
                return _ancestors.Count - i - 1;
        }

        return -1;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Temporary Push(SqlNodeBase node)
    {
        _ancestors.Add( node );
        return new Temporary( _ancestors );
    }

    internal readonly struct Temporary : IDisposable
    {
        private readonly List<SqlNodeBase> _ancestors;

        internal Temporary(List<SqlNodeBase> ancestors)
        {
            _ancestors = ancestors;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _ancestors.RemoveAt( _ancestors.Count - 1 );
        }
    }
}
