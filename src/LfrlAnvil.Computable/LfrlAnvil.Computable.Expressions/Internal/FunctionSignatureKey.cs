using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct FunctionSignatureKey : IEquatable<FunctionSignatureKey>
{
    internal FunctionSignatureKey(IReadOnlyList<Type> parameterTypes)
    {
        ParameterTypes = parameterTypes;
    }

    internal IReadOnlyList<Type> ParameterTypes { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{string.Join( ", ", ParameterTypes.Select( p => p.FullName ) )}]";
    }

    [Pure]
    public override int GetHashCode()
    {
        var count = ParameterTypes.Count;

        var hash = Hash.Default.Add( count );
        for ( var i = 0; i < count; ++i )
            hash = hash.Add( ParameterTypes[i] );

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
        var count = ParameterTypes.Count;
        var otherCount = other.ParameterTypes.Count;

        if ( count != otherCount )
            return false;

        for ( var i = 0; i < count; ++i )
        {
            if ( ParameterTypes[i] != other.ParameterTypes[i] )
                return false;
        }

        return true;
    }
}
