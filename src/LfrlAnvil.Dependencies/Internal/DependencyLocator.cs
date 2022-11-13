using System;
using System.Collections.Generic;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class DependencyLocator : IDependencyLocator
{
    internal DependencyLocator(DependencyScope attachedScope)
    {
        InternalAttachedScope = attachedScope;
        ScopedInstancesByResolverId = new Dictionary<ulong, object>();
        InternalDisposers = new List<DependencyDisposer>();
    }

    public IDependencyScope AttachedScope => InternalAttachedScope;
    internal DependencyScope InternalAttachedScope { get; }
    internal Dictionary<ulong, object> ScopedInstancesByResolverId { get; }
    internal List<DependencyDisposer> InternalDisposers { get; }

    public object Resolve(Type type)
    {
        var result = TryResolve( type );
        if ( result is null )
            throw new MissingDependencyException( type );

        return result;
    }

    public T Resolve<T>()
        where T : class
    {
        var result = TryResolve<T>();
        if ( result is null )
            throw new MissingDependencyException( typeof( T ) );

        return result;
    }

    public object? TryResolve(Type type)
    {
        var result = InternalAttachedScope.InternalContainer.TryResolveDependency( this, type );
        if ( result is null )
            return null;

        if ( type.IsInstanceOfType( result ) )
            return result;

        throw new InvalidDependencyCastException( type, result.GetType() );
    }

    public T? TryResolve<T>()
        where T : class
    {
        var result = InternalAttachedScope.InternalContainer.TryResolveDependency( this, typeof( T ) );
        if ( result is null )
            return null;

        if ( result is T dependency )
            return dependency;

        throw new InvalidDependencyCastException( typeof( T ), result.GetType() );
    }

    internal Chain<OwnedDependencyDisposalException> DisposeInstances()
    {
        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;

        foreach ( var disposer in InternalDisposers )
        {
            var exception = disposer.TryDispose();
            if ( exception is not null )
                exceptions = exceptions.Extend( new OwnedDependencyDisposalException( InternalAttachedScope, exception ) );
        }

        InternalDisposers.Clear();
        ScopedInstancesByResolverId.Clear();
        return exceptions;
    }
}
