using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyResolversStore : IDisposable
{
    internal readonly ReaderWriterLockSlim Lock;
    internal readonly Dictionary<Type, DependencyResolver> Resolvers;

    private DependencyResolversStore(Dictionary<Type, DependencyResolver> resolvers)
    {
        Lock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
        Resolvers = resolvers;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        Lock.DisposeGracefully();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolversStore Create(Dictionary<Type, DependencyResolver> resolvers)
    {
        return new DependencyResolversStore( resolvers );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver? TryGetResolver(Type type)
    {
        Assume.True( Lock.IsReadLockHeld || Lock.IsUpgradeableReadLockHeld );
        return Resolvers.TryGetValue( type, out var resolver ) ? resolver : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddResolver(Type type, DependencyResolver resolver)
    {
        Assume.True( Lock.IsWriteLockHeld );
        Resolvers.Add( type, resolver );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyLifetime? TryGetLifetime(Type type)
    {
        using ( ReadLockSlim.TryEnter( Lock, out var entered ) )
            return entered && Resolvers.TryGetValue( type, out var resolver ) ? resolver.Lifetime : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Type[] GetResolvableTypes()
    {
        using ( ReadLockSlim.TryEnter( Lock, out var entered ) )
        {
            if ( ! entered )
                return Type.EmptyTypes;

            Assume.ContainsAtLeast( Resolvers, 1 );
            var result = new Type[Resolvers.Count];
            Resolvers.Keys.CopyTo( result, 0 );
            return result;
        }
    }
}
