using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

public static class ReinterpretCast
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    [return: NotNullIfNotNull( "value" )]
    public static T? To<T>(object? value)
        where T : class
    {
        Assume.True( value is null or T, ExceptionResources.AssumedInstanceOfType( typeof( T ), value?.GetType(), nameof( value ) ) );
        return Unsafe.As<T>( value );
    }
}
