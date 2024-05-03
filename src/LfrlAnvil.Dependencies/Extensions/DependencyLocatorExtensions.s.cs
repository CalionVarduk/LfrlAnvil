using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Extensions;

/// <summary>
/// Contains <see cref="IDependencyLocator"/> extension methods.
/// </summary>
public static class DependencyLocatorExtensions
{
    /// <summary>
    /// Resolves a dependency of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="locator">Source dependency locator.</param>
    /// <param name="type">Type to resolve.</param>
    /// <returns>Instance of the resolved dependency.</returns>
    /// <exception cref="CircularDependencyReferenceException">When a circular dependency reference has been detected.</exception>
    /// <exception cref="InvalidDependencyCastException">
    /// When the resolved result is not an instance of the provided <paramref name="type"/>.
    /// </exception>
    /// <exception cref="MissingDependencyException">When the provided <paramref name="type"/> could not be resolved.</exception>
    [Pure]
    public static object Resolve(this IDependencyLocator locator, Type type)
    {
        var result = locator.TryResolve( type );
        if ( result is null )
            ExceptionThrower.Throw( new MissingDependencyException( type ) );

        return result;
    }

    /// <summary>
    /// Resolves a dependency of the provided type.
    /// </summary>
    /// <param name="locator">Source dependency locator.</param>
    /// <typeparam name="T">Type to resolve.</typeparam>
    /// <returns>Instance of the resolved dependency.</returns>
    /// <exception cref="CircularDependencyReferenceException">When a circular dependency reference has been detected.</exception>
    /// <exception cref="InvalidDependencyCastException">
    /// When the resolved result is not an instance of the provided type.
    /// </exception>
    /// <exception cref="MissingDependencyException">When the provided type could not be resolved.</exception>
    [Pure]
    public static T Resolve<T>(this IDependencyLocator locator)
        where T : class
    {
        var result = locator.TryResolve<T>();
        if ( result is null )
            ExceptionThrower.Throw( new MissingDependencyException( typeof( T ) ) );

        return result;
    }

    /// <summary>
    /// Attempts to resolve a dependency of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="locator">Source dependency locator.</param>
    /// <param name="type">Type to resolve.</param>
    /// <returns>Instance of the resolved dependency or null when the resolution has failed.</returns>
    /// <exception cref="CircularDependencyReferenceException">When a circular dependency reference has been detected.</exception>
    /// <exception cref="InvalidDependencyCastException">
    /// When the resolved result is not an instance of the provided <paramref name="type"/>.
    /// </exception>
    [Pure]
    public static object? TryResolve(this IDependencyLocator locator, Type type)
    {
        var result = locator.TryResolveUnsafe( type );
        if ( result is null )
            return null;

        if ( type.IsInstanceOfType( result ) )
            return result;

        ExceptionThrower.Throw( new InvalidDependencyCastException( type, result.GetType() ) );
        return default;
    }

    /// <summary>
    /// Attempts to resolve a dependency of the provided type.
    /// </summary>
    /// <param name="locator">Source dependency locator.</param>
    /// <typeparam name="T">Type to resolve.</typeparam>
    /// <returns>Instance of the resolved dependency or null when the resolution has failed.</returns>
    /// <exception cref="CircularDependencyReferenceException">When a circular dependency reference has been detected.</exception>
    /// <exception cref="InvalidDependencyCastException">
    /// When the resolved result is not an instance of the provided type.
    /// </exception>
    [Pure]
    public static T? TryResolve<T>(this IDependencyLocator locator)
        where T : class
    {
        var result = locator.TryResolveUnsafe( typeof( T ) );
        if ( result is null )
            return null;

        if ( result is T dependency )
            return dependency;

        ExceptionThrower.Throw( new InvalidDependencyCastException( typeof( T ), result.GetType() ) );
        return default;
    }
}
