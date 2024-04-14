using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class CycleTrackingTransientDependencyResolver : CycleTrackingDependencyResolver, IResolverFactorySource
{
    internal CycleTrackingTransientDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = factory;
    }

    internal CycleTrackingTransientDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = expression.CreateResolverFactory( this );
    }

    public Func<DependencyScope, object> Factory { get; set; }
    internal override DependencyLifetime Lifetime => DependencyLifetime.Transient;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        using ( TrackCycles( dependencyType ) )
        {
            TryInvokeOnResolvingCallback( dependencyType, scope );
            var result = InvokeFactory( Factory, scope, dependencyType );
            this.TryRegisterTransientDisposer( result, scope );
            return result;
        }
    }
}
