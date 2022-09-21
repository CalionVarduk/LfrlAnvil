using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Extensions;

public static class StructValidatorExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForNullable<T, TResult>(this IValidator<T, TResult> validator)
        where T : struct
    {
        return new ForNullableStructValidator<T, TResult>( validator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForDefaultIfNull<T, TResult>(this IValidator<T, TResult> validator, T defaultValue)
        where T : struct
    {
        return new ForDefaultIfNullStructValidator<T, TResult>( validator, defaultValue );
    }
}
