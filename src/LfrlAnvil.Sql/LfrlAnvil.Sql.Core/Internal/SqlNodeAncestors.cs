using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlNodeAncestors
{
    private readonly List<SqlNodeBase> _ancestors;

    internal SqlNodeAncestors(List<SqlNodeBase> ancestors)
    {
        _ancestors = ancestors;
    }

    public int Count => _ancestors.Count;
    public SqlNodeBase this[int index] => _ancestors[_ancestors.Count - index - 1];

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
