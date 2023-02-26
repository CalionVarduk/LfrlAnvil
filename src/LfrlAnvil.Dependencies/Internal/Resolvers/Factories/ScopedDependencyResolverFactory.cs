using System;
using System.Linq.Expressions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class ScopedDependencyResolverFactory : ImplementorBasedDependencyResolverFactory
{
    internal ScopedDependencyResolverFactory(ImplementorKey implementorKey, IDependencyImplementorBuilder implementorBuilder)
        : base( implementorKey, implementorBuilder, DependencyLifetime.Scoped ) { }

    protected override DependencyResolver CreateFromExpression(
        Expression<Func<DependencyScope, object>> expression,
        UlongSequenceGenerator idGenerator)
    {
        Assume.IsNotNull( ImplementorBuilder, nameof( ImplementorBuilder ) );

        return new ScopedDependencyResolver(
            idGenerator.Generate(),
            ImplementorBuilder.ImplementorType,
            ImplementorBuilder.DisposalStrategy,
            ImplementorBuilder.OnResolvingCallback,
            expression );
    }

    protected override DependencyResolver CreateFromFactory(UlongSequenceGenerator idGenerator)
    {
        Assume.IsNotNull( ImplementorBuilder, nameof( ImplementorBuilder ) );
        Assume.IsNotNull( ImplementorBuilder.Factory, nameof( ImplementorBuilder.Factory ) );

        return new ScopedDependencyResolver(
            idGenerator.Generate(),
            ImplementorBuilder.ImplementorType,
            ImplementorBuilder.DisposalStrategy,
            ImplementorBuilder.OnResolvingCallback,
            ImplementorBuilder.Factory );
    }
}
