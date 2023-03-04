using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class RangeDependencyResolverFactory : DependencyResolverFactory
{
    internal RangeDependencyResolverFactory(
        ImplementorKey implementorKey,
        Action<Type, IDependencyScope>? onResolvingCallback,
        DependencyResolverFactory[]? factories)
        : base( implementorKey, DependencyLifetime.Transient )
    {
        Assume.Equals( implementorKey.Value.Type.IsGenericType, true, nameof( implementorKey.Value.Type.IsGenericType ) );
        Assume.Equals(
            implementorKey.Value.Type.GetGenericTypeDefinition(),
            typeof( IEnumerable<> ),
            nameof( implementorKey.Value.Type.GetGenericTypeDefinition ) );

        ElementType = implementorKey.Value.Type.GetGenericArguments()[0];
        OnResolvingCallback = onResolvingCallback;
        Factories = factories;
    }

    internal DependencyResolverFactory[]? Factories { get; }
    internal Type ElementType { get; }
    internal Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    [Pure]
    internal override Chain<DependencyResolverFactory> GetCaptiveDependencyFactories(DependencyLifetime lifetime)
    {
        var result = Chain<DependencyResolverFactory>.Empty;
        if ( Factories is null )
            return result;

        foreach ( var f in Factories )
            result = result.Extend( f.GetCaptiveDependencyFactories( lifetime ) );

        return result;
    }

    [Pure]
    internal override bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        if ( Factories is null )
            return false;

        foreach ( var f in Factories )
        {
            if ( f.IsCaptiveDependencyOf( lifetime ) )
                return true;
        }

        return false;
    }

    [Pure]
    protected override Chain<DependencyContainerBuildMessages> CreateMessages()
    {
        var result = Chain<DependencyContainerBuildMessages>.Empty;
        if ( Factories is null )
            return result;

        foreach ( var f in Factories )
            result = result.Extend( f.GetMessages() );

        return result;
    }

    protected override bool IsCreationMethodValid(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( Factories is null )
            return true;

        foreach ( var f in Factories )
            f.PrepareCreationMethod( idGenerator, availableDependencies, configuration );

        return true;
    }

    protected override bool AreRequiredDependenciesValid(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( Factories is null )
            return true;

        foreach ( var f in Factories )
            f.ValidateRequiredDependencies( availableDependencies, configuration );

        return true;
    }

    protected override void OnCircularDependencyDetected(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 1, nameof( path ) );
        Assume.IsNotNull( Factories, nameof( Factories ) );

        var reachedFrom = path[^1].ReachedFrom;
        path.Add( default );

        foreach ( var f in Factories )
        {
            path[^1] = new DependencyGraphNode( reachedFrom, f );
            DetectCircularDependencies( f, path );
        }

        path.RemoveLast();
    }

    protected override void DetectCircularDependenciesInChildren(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 2, nameof( path ) );
        if ( Factories is null )
            return;

        var reachedFrom = path[^2].ReachedFrom;
        foreach ( var f in Factories )
        {
            path[^1] = new DependencyGraphNode( reachedFrom, f );
            DetectCircularDependencies( f, path );
        }
    }

    protected override DependencyResolver CreateResolver(UlongSequenceGenerator idGenerator)
    {
        Assume.Conditional(
            Factories is not null,
            () => Assume.True(
                Factories!.All( f => ! f.HasState( DependencyResolverFactoryState.Invalid ) ),
                "All range dependency resolver factory elements must be valid." ) );

        var resolver = new TransientDependencyResolver(
            id: idGenerator.Generate(),
            implementorType: ImplementorKey.Value.Type,
            disposalStrategy: DependencyImplementorDisposalStrategy.RenounceOwnership(),
            onResolvingCallback: OnResolvingCallback,
            expression: CreateExpression( idGenerator ) );

        return resolver;
    }

    [Pure]
    private Expression<Func<DependencyScope, object>> CreateExpression(UlongSequenceGenerator idGenerator)
    {
        var (expressionBuilder, factoryCount) = CreateExpressionBuilder();
        for ( var i = 0; i < factoryCount; ++i )
        {
            Assume.IsNotNull( Factories, nameof( Factories ) );
            expressionBuilder.AddDependencyResolverFactoryResolution( ElementType, $"e{i}", Factories[i], idGenerator );
        }

        var arrayInit = factoryCount > 0
            ? Expression.NewArrayInit( ElementType, expressionBuilder.GetVariableRange( factoryCount ) )
            : ExpressionBuilder.CreateArrayEmptyCallExpression( ElementType );

        var result = expressionBuilder.Build( arrayInit );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ExpressionBuilder Builder, int FactoryCount) CreateExpressionBuilder()
    {
        var factoryCount = Factories?.Length ?? 0;
        var hasRequiredValueTypeDependency = ElementType.IsValueType && Nullable.GetUnderlyingType( ElementType ) is null;
        var builder = new ExpressionBuilder( factoryCount, 0, hasRequiredValueTypeDependency, ImplementorKey.Value.Type );
        return (builder, factoryCount);
    }
}
