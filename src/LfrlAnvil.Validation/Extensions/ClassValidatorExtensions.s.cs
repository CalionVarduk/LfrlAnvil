using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Extensions;

public static class RefValidatorExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForNullable<T, TResult>(this IValidator<T, TResult> validator)
        where T : class
    {
        return new ForNullableRefValidator<T, TResult>( validator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T?, TResult> ForDefaultIfNull<T, TResult>(this IValidator<T, TResult> validator, T defaultValue)
        where T : class
    {
        return new ForDefaultIfNullRefValidator<T, TResult>( validator, defaultValue );
    }
}
