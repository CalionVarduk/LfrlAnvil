using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal static class Helpers
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void DisposeGracefully(this ReaderWriterLockSlim @lock)
    {
        var spinWait = new SpinWait();
        while ( true )
        {
            if ( @lock.IsWriteLockHeld || @lock.IsUpgradeableReadLockHeld || @lock.IsReadLockHeld )
                @lock.Dispose();

            try
            {
                @lock.Dispose();
                break;
            }
            catch ( SynchronizationLockException )
            {
                spinWait.SpinOnce();
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Func<DependencyScope, object> CreateResolverFactory(
        this Expression<Func<DependencyScope, object>> expression,
        IResolverFactorySource source)
    {
        Func<DependencyScope, object>? compiled = null;
        return scope =>
        {
            using ( ExclusiveLock.Enter( expression ) )
            {
                if ( compiled is not null )
                {
                    Assume.Equals( compiled, source.Factory );
                    return compiled( scope );
                }

                compiled = expression.Compile();
                source.Factory = compiled;
            }

            Assume.Equals( compiled, source.Factory );
            return compiled( scope );
        };
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object CreateScopedInstance(
        this DependencyResolver resolver,
        Func<DependencyScope, object> factory,
        DependencyScope scope,
        Type dependencyType)
    {
        Assume.True( scope.Lock.IsWriteLockHeld );

        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( scope.ScopedInstancesByResolverId, resolver.Id, out var exists )!;
        if ( exists )
            return result;

        try
        {
            result = resolver.InvokeFactory( factory, scope, dependencyType );
        }
        catch
        {
            scope.ScopedInstancesByResolverId.Remove( resolver.Id );
            throw;
        }

        var disposer = resolver.DisposalStrategy.TryCreateDisposer( result );
        if ( disposer is not null )
            scope.InternalDisposers.Add( disposer.Value );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void TryRegisterTransientDisposer(this DependencyResolver resolver, object result, DependencyScope scope)
    {
        var disposer = resolver.DisposalStrategy.TryCreateDisposer( result );
        if ( disposer is null )
            return;

        using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposed )
            {
                disposer.Value.TryDispose();
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );
            }

            scope.InternalDisposers.Add( disposer.Value );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object? TryFindAncestorScopedSingletonInstance(this DependencyResolver resolver, DependencyScope? scope)
    {
        while ( scope is not null )
        {
            using ( ReadLockSlim.TryEnter( scope.Lock, out var entered ) )
            {
                if ( ! entered || scope.IsDisposed )
                    ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );

                if ( scope.ScopedInstancesByResolverId.TryGetValue( resolver.Id, out var result ) )
                    return result;
            }

            scope = scope.InternalParentScope;
        }

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MethodInfo FindResolverCreateMethod(Type resolverType)
    {
        var result = resolverType.GetMethod( nameof( DependencyResolver.Create ), BindingFlags.Instance | BindingFlags.NonPublic );
        Assume.IsNotNull( result );
        return result;
    }
}
