using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

public static class Maybe
{
    public static readonly Nil None = Nil.Instance;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> Some<T>(T? value)
        where T : notnull
    {
        if ( Generic<T>.IsNull( value ) )
            throw new ArgumentNullException( nameof( value ) );

        return new Maybe<T>( value! );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Maybe<> ) );
        return result.Length == 0 ? null : result[0];
    }
}