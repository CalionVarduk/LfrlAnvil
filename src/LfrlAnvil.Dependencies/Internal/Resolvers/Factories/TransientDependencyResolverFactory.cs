using System;
using System.Linq.Expressions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class TransientDependencyResolverFactory : RegisteredDependencyResolverFactory
{
    internal TransientDependencyResolverFactory(ImplementorKey implementorKey, IDependencyImplementorBuilder implementorBuilder)
        : base( implementorKey, implementorBuilder, DependencyLifetime.Transient ) { }

    protected override DependencyResolver CreateFromExpression(
        Expression<Func<DependencyScope, object>> expression,
        UlongSequenceGenerator idGenerator)
    {
        Assume.IsNotNull( ImplementorBuilder );

        return ImplementorBuilder.OnResolvingCallback is null && ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback is null
            ? new TransientDependencyResolver(
                idGenerator.Generate(),
                ImplementorBuilder.ImplementorType,
                ImplementorBuilder.DisposalStrategy,
                expression )
            : new CycleTrackingTransientDependencyResolver(
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

        return new CycleTrackingTransientDependencyResolver(
            idGenerator.Generate(),
            ImplementorBuilder.ImplementorType,
            ImplementorBuilder.DisposalStrategy,
            ImplementorBuilder.OnResolvingCallback,
            ImplementorBuilder.Factory );
    }
}
