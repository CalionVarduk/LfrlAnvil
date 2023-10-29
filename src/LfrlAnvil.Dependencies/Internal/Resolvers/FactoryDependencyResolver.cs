using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal abstract class FactoryDependencyResolver : DependencyResolver
{
    internal FactoryDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = factory;
    }

    internal FactoryDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = scope =>
        {
            var result = expression.Compile();
            Factory = result;
            return result( scope );
        };
    }

    protected Func<DependencyScope, object>? Factory { get; private set; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void ClearFactory()
    {
        Assume.IsNotNull( Factory );
        Factory = null;
    }
}
