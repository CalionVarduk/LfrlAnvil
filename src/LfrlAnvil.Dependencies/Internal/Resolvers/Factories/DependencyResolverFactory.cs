using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal class DependencyResolverFactory
{
    private DependencyResolver? _resolver;
    private bool _messagesCreated;

    protected DependencyResolverFactory(ImplementorKey implementorKey, DependencyLifetime lifetime)
    {
        Assume.IsDefined( lifetime );
        Lifetime = lifetime;
        State = DependencyResolverFactoryState.Created;
        ImplementorKey = implementorKey;
        _resolver = null;
        _messagesCreated = false;
    }

    internal DependencyResolverFactoryState State { get; private set; }
    internal DependencyLifetime Lifetime { get; }
    internal ImplementorKey ImplementorKey { get; }
    internal IInternalDependencyKey InternalImplementorKey => ReinterpretCast.To<IInternalDependencyKey>( ImplementorKey.Value );
    internal bool IsFinished => HasState( DependencyResolverFactoryState.Finished );

    [Pure]
    public override string ToString()
    {
        return $"[{ImplementorKey}]: ({Lifetime}, {State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolverFactory Create(
        ImplementorKey implementorKey,
        DependencyLifetime lifetime,
        IDependencyImplementorBuilder implementorBuilder)
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolverFactory CreateFinished(
        ImplementorKey implementorKey,
        DependencyLifetime lifetime,
        DependencyResolver resolver)
    {
        var result = new DependencyResolverFactory( implementorKey, lifetime );
        result.Finish( resolver );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolverFactory CreateInvalid(ImplementorKey implementorKey, DependencyLifetime lifetime)
    {
        var result = new DependencyResolverFactory( implementorKey, lifetime );
        result.FinishAsInvalid();
        return result;
    }

    internal void PrepareCreationMethod(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( State != DependencyResolverFactoryState.Created )
            return;

        if ( ! IsCreationMethodValid( idGenerator, availableDependencies, configuration ) )
            FinishAsInvalid();
        else if ( ! IsFinished )
            SetState( DependencyResolverFactoryState.Validatable );
    }

    internal void ValidateRequiredDependencies(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( State != DependencyResolverFactoryState.Validatable )
            return;

        if ( AreRequiredDependenciesValid( availableDependencies, configuration ) )
            SetState( DependencyResolverFactoryState.ValidatedRequiredDependencies );
        else
            FinishAsInvalid();
    }

    internal void ValidateCircularDependencies(List<DependencyGraphNode> pathBuffer)
    {
        if ( State != DependencyResolverFactoryState.ValidatedRequiredDependencies )
            return;

        Assume.IsEmpty( pathBuffer );

        pathBuffer.Add( new DependencyGraphNode( null, this ) );
        DetectCircularDependencies( pathBuffer );

        Assume.ContainsExactly( pathBuffer, 1 );
        pathBuffer.Clear();
    }

    internal void Build(UlongSequenceGenerator idGenerator)
    {
        if ( IsFinished )
            return;

        var resolver = CreateResolver( idGenerator );
        Finish( resolver );
    }

    [Pure]
    internal virtual Chain<DependencyResolverFactory> GetCaptiveDependencyFactories(DependencyLifetime lifetime)
    {
        return Chain<DependencyResolverFactory>.Empty;
    }

    [Pure]
    internal virtual bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        return false;
    }

    [Pure]
    internal Chain<DependencyContainerBuildMessages> GetMessages()
    {
        if ( _messagesCreated )
            return Chain<DependencyContainerBuildMessages>.Empty;

        _messagesCreated = true;
        return CreateMessages();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver GetResolver()
    {
        Assume.IsNotNull( _resolver );
        return _resolver;
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddState(DependencyResolverFactory other, DependencyResolverFactoryState state)
    {
        other.AddState( state );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void DetectCircularDependencies(DependencyResolverFactory other, List<DependencyGraphNode> path)
    {
        if ( ! other.HasAnyState( DependencyResolverFactoryState.Validated | DependencyResolverFactoryState.Finished ) )
            other.DetectCircularDependencies( path );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void SetState(DependencyResolverFactoryState state)
    {
        Assume.False( IsFinished );
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

    protected void DetectCircularDependencies(List<DependencyGraphNode> path)
    {
        if ( HasAnyState( DependencyResolverFactoryState.CanRegisterCircularDependency ) )
        {
            Assume.ContainsAtLeast( path, 2 );
            Assume.True( ReferenceEquals( this, path[^1].Factory ) );
            OnCircularDependencyDetected( path );
            return;
        }

        Assume.Equals( State, DependencyResolverFactoryState.ValidatedRequiredDependencies );
        SetState( DependencyResolverFactoryState.ValidatingCircularDependencies );

        path.Add( default );
        DetectCircularDependenciesInChildren( path );
        path.RemoveLast();

        if ( HasState( DependencyResolverFactoryState.CircularDependenciesDetected ) )
            FinishWithCircularDependencies();
        else
            SetState( DependencyResolverFactoryState.Validated );
    }

    [Pure]
    protected virtual Chain<DependencyContainerBuildMessages> CreateMessages()
    {
        return Chain<DependencyContainerBuildMessages>.Empty;
    }

    protected virtual bool IsCreationMethodValid(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        return true;
    }

    protected virtual bool AreRequiredDependenciesValid(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        return true;
    }

    protected virtual void OnCircularDependencyDetected(List<DependencyGraphNode> path) { }

    protected virtual void DetectCircularDependenciesInChildren(List<DependencyGraphNode> path) { }

    protected virtual DependencyResolver CreateResolver(UlongSequenceGenerator idGenerator)
    {
        throw new InvalidOperationException( nameof( CreateResolver ) + " method must be overriden." );
    }
}
