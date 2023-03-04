using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Extensions;

public static class DependencyLocatorExtensions
{
    [Pure]
    public static object Resolve(this IDependencyLocator locator, Type type)
    {
        var result = locator.TryResolve( type );
        if ( result is null )
            throw new MissingDependencyException( type );

        return result;
    }

    [Pure]
    public static T Resolve<T>(this IDependencyLocator locator)
        where T : class
    {
        var result = locator.TryResolve<T>();
        if ( result is null )
            throw new MissingDependencyException( typeof( T ) );

        return result;
    }

    [Pure]
    public static object? TryResolve(this IDependencyLocator locator, Type type)
    {
        var result = locator.TryResolveUnsafe( type );
        if ( result is null )
            return null;

        if ( type.IsInstanceOfType( result ) )
            return result;

        throw new InvalidDependencyCastException( type, result.GetType() );
    }

    [Pure]
    public static T? TryResolve<T>(this IDependencyLocator locator)
        where T : class
    {
        var result = locator.TryResolveUnsafe( typeof( T ) );
        if ( result is null )
            return null;

        if ( result is T dependency )
            return dependency;

        throw new InvalidDependencyCastException( typeof( T ), result.GetType() );
    }
}
