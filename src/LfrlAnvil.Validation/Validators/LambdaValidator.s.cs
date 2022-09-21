using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Validation.Validators;

public static class LambdaValidator<TResult>
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Create<T>(Func<T, Chain<TResult>> validator)
    {
        return new Lambda<T>( validator );
    }

    private sealed class Lambda<T> : IValidator<T, TResult>
    {
        private readonly Func<T, Chain<TResult>> _validator;

        internal Lambda(Func<T, Chain<TResult>> validator)
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
