using System;
using System.Collections.Generic;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyLocator : IDependencyLocator
{
    internal DependencyLocator(DependencyScope attachedScope, Dictionary<Type, DependencyResolver> resolvers)
    {
        InternalAttachedScope = attachedScope;
        Resolvers = resolvers;
    }

    public IDependencyScope AttachedScope => InternalAttachedScope;
    internal DependencyScope InternalAttachedScope { get; }
    internal Dictionary<Type, DependencyResolver> Resolvers { get; }

    Type? IDependencyLocator.KeyType => null;
    object? IDependencyLocator.Key => null;
    bool IDependencyLocator.IsKeyed => false;

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
}

internal sealed class DependencyLocator<TKey> : DependencyLocator, IDependencyLocator<TKey>
    where TKey : notnull
{
    internal DependencyLocator(TKey key, DependencyScope attachedScope, Dictionary<Type, DependencyResolver> resolvers)
        : base( attachedScope, resolvers )
    {
        Key = key;
    }

    public TKey Key { get; }

    Type IDependencyLocator.KeyType => typeof( TKey );
    object IDependencyLocator.Key => Key;
    bool IDependencyLocator.IsKeyed => true;
}
