using System;
using System.Linq.Expressions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class ScopedSingletonDependencyResolverFactory : RegisteredDependencyResolverFactory
{
    internal ScopedSingletonDependencyResolverFactory(
        ImplementorKey implementorKey,
        IDependencyImplementorBuilder implementorBuilder)
        : base( implementorKey, implementorBuilder, DependencyLifetime.ScopedSingleton ) { }

    protected override DependencyResolver CreateFromExpression(
        Expression<Func<DependencyScope, object>> expression,
        UlongSequenceGenerator idGenerator)
    {
        Assume.IsNotNull( ImplementorBuilder );

        return new ScopedSingletonDependencyResolver(
            idGenerator.Generate(),
            ImplementorBuilder.ImplementorType,
            ImplementorBuilder.DisposalStrategy,
            ImplementorBuilder.OnResolvingCallback,
            expression );
    }

    protected override DependencyResolver CreateFromFactory(UlongSequenceGenerator idGenerator)
    {
        Assume.IsNotNull( ImplementorBuilder );
        Assume.IsNotNull( ImplementorBuilder.Factory );

        return new ScopedSingletonDependencyResolver(
            idGenerator.Generate(),
            ImplementorBuilder.ImplementorType,
            ImplementorBuilder.DisposalStrategy,
            ImplementorBuilder.OnResolvingCallback,
            ImplementorBuilder.Factory );
    }
}
