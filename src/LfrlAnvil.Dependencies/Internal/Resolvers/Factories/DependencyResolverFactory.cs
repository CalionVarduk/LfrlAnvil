// Copyright 2024-2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal class DependencyResolverFactory
{
    private DependencyResolver? _resolver;
    private bool _messagesCreated;

    protected DependencyResolverFactory(ImplementorKey implementorKey, DependencyLifetime lifetime, bool isOpenGeneric)
    {
        Assume.IsDefined( lifetime );
        Lifetime = lifetime;
        State = DependencyResolverFactoryState.Created;
        ImplementorKey = implementorKey;
        IsOpenGeneric = isOpenGeneric;
        _resolver = null;
        _messagesCreated = false;
    }

    internal DependencyResolverFactoryState State { get; private set; }
    internal DependencyLifetime Lifetime { get; }
    internal ImplementorKey ImplementorKey { get; }
    internal bool IsOpenGeneric { get; }
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
        DependencyImplementorBuilder implementorBuilder)
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
        var result = new DependencyResolverFactory( implementorKey, lifetime, isOpenGeneric: false );
        result.Finish( resolver );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolverFactory CreateInvalid(
        ImplementorKey implementorKey,
        DependencyLifetime lifetime,
        bool isOpenGeneric = false)
    {
        var result = new DependencyResolverFactory( implementorKey, lifetime, isOpenGeneric );
        result.FinishAsInvalid();
        return result;
    }

    internal void PrepareCreationMethod(
        UlongSequenceGenerator idGenerator,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration)
    {
        if ( State != DependencyResolverFactoryState.Created )
            return;

        if ( ! IsCreationMethodValid( idGenerator, availableDependencies, configuration ) )
            FinishAsInvalid();
        else if ( ! IsFinished )
            SetState( DependencyResolverFactoryState.Validatable );
    }

    internal void ValidateRequiredDependencies(
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        DependencyContainerConfigurationBuilder configuration)
    {
        if ( State != DependencyResolverFactoryState.Validatable )
            return;

        if ( AreRequiredDependenciesValid( in @params, dynamicResolverFactories, configuration ) )
            SetState( DependencyResolverFactoryState.ValidatedRequiredDependencies );
        else
            FinishAsInvalid();
    }

    internal void ValidateCircularDependencies(ref ListSlim<DependencyGraphNode> pathBuffer)
    {
        if ( State != DependencyResolverFactoryState.ValidatedRequiredDependencies )
            return;

        Assume.Equals( pathBuffer.Count, 0 );

        pathBuffer.Add( new DependencyGraphNode( null, this ) );
        DetectCircularDependencies( ref pathBuffer );

        Assume.Equals( pathBuffer.Count, 1 );
        pathBuffer.Clear();
    }

    internal void Build(UlongSequenceGenerator idGenerator, DependencyContainerConfigurationBuilder configuration)
    {
        if ( IsFinished )
            return;

        var resolver = CreateResolver( idGenerator, configuration );
        Finish( resolver );
    }

    internal virtual void RegisterResolver(
        IDependencyKey dependencyKey,
        in DependencyResolversStore globalResolvers,
        in KeyedDependencyResolversStore keyedResolversStore)
    {
        var dependencyStore = ReinterpretCast.To<IInternalDependencyKey>( dependencyKey )
            .GetTargetResolversStore( in globalResolvers, in keyedResolversStore );

        var resolver = GetResolver();
        dependencyStore.Resolvers.TryAdd( dependencyKey.Type, resolver );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolverFactory Close(
        IInternalDependencyKey dependencyKey,
        in DependencyLocatorBuilderExtractionParams @params)
    {
        return Close( dependencyKey, in @params, @params.ResolverFactories );
    }

    internal virtual DependencyResolverFactory Close(
        IInternalDependencyKey dependencyKey,
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories)
    {
        throw new NotSupportedException( nameof( Close ) + " method is not supported." );
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
    protected static void DetectCircularDependencies(DependencyResolverFactory other, ref ListSlim<DependencyGraphNode> path)
    {
        if ( ! other.HasAnyState( DependencyResolverFactoryState.Validated | DependencyResolverFactoryState.Finished ) )
            other.DetectCircularDependencies( ref path );
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
            DependencyResolverFactoryState.Finished
            | DependencyResolverFactoryState.Invalid
            | DependencyResolverFactoryState.CircularDependenciesDetected );
    }

    protected void DetectCircularDependencies(ref ListSlim<DependencyGraphNode> path)
    {
        if ( HasAnyState( DependencyResolverFactoryState.CanRegisterCircularDependency ) )
        {
            Assume.IsGreaterThanOrEqualTo( path.Count, 2 );
            Assume.True( ReferenceEquals( this, path[^1].Factory ) );
            OnCircularDependencyDetected( ref path );
            return;
        }

        Assume.Equals( State, DependencyResolverFactoryState.ValidatedRequiredDependencies );
        SetState( DependencyResolverFactoryState.ValidatingCircularDependencies );

        path.Add( default );
        DetectCircularDependenciesInChildren( ref path );
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
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration)
    {
        return true;
    }

    protected virtual bool AreRequiredDependenciesValid(
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        DependencyContainerConfigurationBuilder configuration)
    {
        return true;
    }

    protected virtual void OnCircularDependencyDetected(ref ListSlim<DependencyGraphNode> path) { }

    protected virtual void DetectCircularDependenciesInChildren(ref ListSlim<DependencyGraphNode> path) { }

    protected virtual DependencyResolver CreateResolver(
        UlongSequenceGenerator idGenerator,
        DependencyContainerConfigurationBuilder configuration)
    {
        throw new InvalidOperationException( nameof( CreateResolver ) + " method must be overriden." );
    }
}
