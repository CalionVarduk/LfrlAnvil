using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal abstract class DependencyResolver
{
    private bool _isResolving;

    protected DependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback)
    {
        Id = id;
        ImplementorType = implementorType;
        DisposalStrategy = disposalStrategy;
        OnResolvingCallback = onResolvingCallback;
        _isResolving = false;
    }

    internal ulong Id { get; }
    internal Type ImplementorType { get; }
    internal DependencyImplementorDisposalStrategy DisposalStrategy { get; }
    internal Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    internal object Create(DependencyScope scope, Type dependencyType)
    {
        if ( _isResolving )
            throw new CircularDependencyReferenceException( dependencyType, ImplementorType );

        OnResolvingCallback?.Invoke( dependencyType, scope );
        _isResolving = true;

        try
        {
            return CreateInternal( scope );
        }
        catch ( CircularDependencyReferenceException exc )
        {
            throw new CircularDependencyReferenceException( dependencyType, ImplementorType, exc );
        }
        finally
        {
            _isResolving = false;
        }
    }

    protected abstract object CreateInternal(DependencyScope scope);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void SetupDisposalStrategy(DependencyScope scope, object instance)
    {
        switch ( DisposalStrategy.Type )
        {
            case DependencyImplementorDisposalStrategyType.UseDisposableInterface:
            {
                if ( instance is IDisposable disposable )
                    scope.InternalLocator.InternalDisposers.Add( new DependencyDisposer( disposable, callback: null ) );

                break;
            }
            case DependencyImplementorDisposalStrategyType.UseCallback:
            {
                Assume.IsNotNull( DisposalStrategy.Callback, nameof( DisposalStrategy.Callback ) );
                scope.InternalLocator.InternalDisposers.Add( new DependencyDisposer( instance, DisposalStrategy.Callback ) );
                break;
            }
        }
    }
}
