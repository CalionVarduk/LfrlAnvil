using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Extensions;

/// <summary>
/// Contains <see cref="IValidator{T,TResult}"/> extension methods for value types.
/// </summary>
public static class StructValidatorExtensions
{
    /// <summary>
    /// Creates a new <see cref="ForNullableStructValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="ForNullableStructValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForNullable<T, TResult>(this IValidator<T, TResult> validator)
        where T : struct
    {
        return new ForNullableStructValidator<T, TResult>( validator );
    }

    /// <summary>
    /// Creates a new <see cref="ForDefaultIfNullStructValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="defaultValue">Default value to use instead of a null object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="ForDefaultIfNullStructValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForDefaultIfNull<T, TResult>(this IValidator<T, TResult> validator, T defaultValue)
        where T : struct
    {
        return new ForDefaultIfNullStructValidator<T, TResult>( validator, defaultValue );
    }
}
