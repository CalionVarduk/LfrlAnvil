using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Creates instances of <see cref="IValidator{T,TResult}"/> type through delegates.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public static class LambdaValidator<TResult>
{
    /// <summary>
    /// Creates a new <see cref="IValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Validation delegate.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Create<T>(Func<T, Chain<TResult>> validator)
    {
        return new Validator<T>( validator );
    }

    private sealed class Validator<T> : IValidator<T, TResult>
    {
        private readonly Func<T, Chain<TResult>> _validator;

        internal Validator(Func<T, Chain<TResult>> validator)
        {
            _validator = validator;
        }

        [Pure]
        public Chain<TResult> Validate(T obj)
        {
            return _validator( obj );
        }
    }
}
