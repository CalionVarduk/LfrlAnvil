using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class TransientDependencyResolver : DependencyResolver, IResolverFactorySource
{
    internal TransientDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy )
    {
        Factory = expression.CreateResolverFactory( this );
    }

    public Func<DependencyScope, object> Factory { get; set; }
    internal override DependencyLifetime Lifetime => DependencyLifetime.Transient;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        var result = InvokeFactory( Factory, scope, dependencyType );
        this.TryRegisterTransientDisposer( result, scope );
        return result;
    }
}
