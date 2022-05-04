using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping.Internal
{
    public readonly struct MappingStore
    {
        private MappingStore(Delegate fastDelegate, Delegate slowDelegate)
        {
            FastDelegate = fastDelegate;
            SlowDelegate = slowDelegate;
        }

        public Delegate FastDelegate { get; }
        public Delegate SlowDelegate { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Func<TSource, ITypeMapper, TDestination> GetDelegate<TSource, TDestination>()
        {
            return (Func<TSource, ITypeMapper, TDestination>)FastDelegate;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Func<object, ITypeMapper, TDestination> GetDelegate<TDestination>()
        {
            return (Func<object, ITypeMapper, TDestination>)SlowDelegate;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Func<object, ITypeMapper, object> GetDelegate()
        {
            return (Func<object, ITypeMapper, object>)SlowDelegate;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MappingStore Create<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            Func<object, ITypeMapper, TDestination> slowMapping = (source, provider) => mapping( (TSource)source, provider );
            return new MappingStore( mapping, slowMapping );
        }
    }
}
