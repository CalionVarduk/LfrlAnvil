using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// Contains methods for unsafe reinterpret casting.
/// </summary>
public static class ReinterpretCast
{
    /// <summary>
    /// Reinterprets the provided <paramref name="value"/> as an object of the desired reference type.
    /// </summary>
    /// <param name="value">Value to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns></returns>
    /// <remarks>This method is unsafe, use with caution. See <see cref="Unsafe.As{T}(Object)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    [return: NotNullIfNotNull( "value" )]
    public static T? To<T>(object? value)
        where T : class
    {
        Debug.Assert( value is null or T, ExceptionResources.AssumedInstanceOfType( typeof( T ), value?.GetType(), nameof( value ) ) );
        return Unsafe.As<T>( value );
    }
}
