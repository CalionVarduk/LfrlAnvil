using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class DependencyResolverFactory
{
    private DependencyResolver? _resolver;

    protected DependencyResolverFactory(DependencyLifetime lifetime)
    {
        Assume.IsDefined( lifetime, nameof( lifetime ) );
        Lifetime = lifetime;
        State = DependencyResolverFactoryState.Created;
        _resolver = null;
    }

    protected DependencyResolverFactory(DependencyResolver resolver)
    {
        Lifetime = DependencyLifetime.Singleton;
        State = DependencyResolverFactoryState.Internal | DependencyResolverFactoryState.Finished;
        _resolver = resolver;
    }

    internal DependencyResolverFactoryState State { get; private set; }
    internal DependencyLifetime Lifetime { get; }
    internal bool IsInternal => HasState( DependencyResolverFactoryState.Internal );
    internal bool IsFinished => HasState( DependencyResolverFactoryState.Finished );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolverFactory Create(
        ImplementorKey implementorKey,
        IDependencyImplementorBuilder implementorBuilder,
        DependencyLifetime lifetime)
    {
        DependencyResolverFactory result = lifetime switch
        {
            DependencyLifetime.Singleton => new SingletonDependencyResolverFactory( implementorKey, implementorBuilder ),
            DependencyLifetime.ScopedSingleton => new ScopedSingletonDependencyResolverFactory( implementorKey, implementorBuilder ),
            DependencyLifetime.Scoped => new ScopedDependencyResolverFactory( implementorKey, implementorBuilder ),
            _ => new TransientDependencyResolverFactory( implementorKey, implementorBuilder )
        };

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool HasState(DependencyResolverFactoryState state)
    {
        return (State & state) == state;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool HasAnyState(DependencyResolverFactoryState state)
    {
        return (State & state) != DependencyResolverFactoryState.Created;
    }

    [Pure]
    internal virtual bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        return false;
    }

    [Pure]
    internal virtual DependencyContainerBuildMessages? GetMessages()
    {
        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver GetResolver()
    {
        Assume.IsNotNull( _resolver, nameof( _resolver ) );
        return _resolver;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void SetState(DependencyResolverFactoryState state)
    {
        Assume.Equals( IsFinished, false, nameof( IsFinished ) );
        State = state;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void AddState(DependencyResolverFactoryState state)
    {
        SetState( State | state );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void Finish(DependencyResolver resolver)
    {
        SetState( DependencyResolverFactoryState.Finished );
        _resolver = resolver;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void FinishAsInvalid()
    {
        SetState( DependencyResolverFactoryState.Finished | DependencyResolverFactoryState.Invalid );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void FinishWithCircularDependencies()
    {
        SetState(
            DependencyResolverFactoryState.Finished |
            DependencyResolverFactoryState.Invalid |
            DependencyResolverFactoryState.CircularDependenciesDetected );
    }
}
