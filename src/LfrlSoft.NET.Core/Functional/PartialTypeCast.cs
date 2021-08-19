using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Functional
{
    public readonly struct PartialTypeCast<TSource>
    {
        public readonly TSource Value;

        internal PartialTypeCast(TSource value)
        {
            Value = value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TypeCast<TSource, TDestination> To<TDestination>()
        {
            return new TypeCast<TSource, TDestination>( Value );
        }
    }
}
