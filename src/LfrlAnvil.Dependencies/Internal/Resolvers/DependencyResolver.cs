using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal abstract class DependencyResolver
{
    protected DependencyResolver(ulong id, Type implementorType, DependencyImplementorDisposalStrategy disposalStrategy)
    {
        Id = id;
        ImplementorType = implementorType;
        DisposalStrategy = disposalStrategy;
    }

    internal ulong Id { get; }
    internal Type ImplementorType { get; }
    internal abstract DependencyLifetime Lifetime { get; }
    internal DependencyImplementorDisposalStrategy DisposalStrategy { get; }

    internal abstract object Create(DependencyScope scope, Type dependencyType);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal object InvokeFactory(Func<DependencyScope, object> factory, DependencyScope scope, Type dependencyType)
    {
        try
        {
            return factory( scope );
        }
        catch ( CircularDependencyReferenceException exc )
        {
            ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, ImplementorType, exc ) );
            return default!;
        }
    }
}
