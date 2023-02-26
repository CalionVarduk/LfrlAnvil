using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct FunctionSignatureKey : IEquatable<FunctionSignatureKey>
{
    internal FunctionSignatureKey(IReadOnlyList<Expression> parameters)
    {
        Parameters = parameters;
    }

    internal IReadOnlyList<Expression> Parameters { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{string.Join( ", ", Parameters.Select( static e => e.Type.GetDebugString() ) )}]";
    }

    [Pure]
    public override int GetHashCode()
    {
        var count = Parameters.Count;

        var hash = Hash.Default.Add( count );
        for ( var i = 0; i < count; ++i )
            hash = hash.Add( Parameters[i].Type );

        return hash.Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is FunctionSignatureKey k && Equals( k );
    }

    [Pure]
    public bool Equals(FunctionSignatureKey other)
    {
        var count = Parameters.Count;
        var otherCount = other.Parameters.Count;

        if ( count != otherCount )
            return false;

        for ( var i = 0; i < count; ++i )
        {
            if ( Parameters[i].Type != other.Parameters[i].Type )
                return false;
        }

        return true;
    }
}
