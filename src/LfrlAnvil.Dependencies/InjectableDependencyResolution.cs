using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a custom parameter or member resolution.
/// </summary>
/// <typeparam name="T">Dependency type.</typeparam>
public readonly struct InjectableDependencyResolution<T>
    where T : class, ICustomAttributeProvider
{
    private InjectableDependencyResolution(
        Func<T, bool> predicate,
        Expression<Func<IDependencyScope, object>>? factory,
        IDependencyKey? implementorKey)
    {
        Predicate = predicate;
        Factory = factory;
        ImplementorKey = implementorKey;
    }

    /// <summary>
    /// Predicate used for locating the desired parameter or member, denoted by the predicate returning <b>true</b>.
    /// </summary>
    public Func<T, bool> Predicate { get; }

    /// <summary>
    /// Custom resolution factory.
    /// </summary>
    public Expression<Func<IDependencyScope, object>>? Factory { get; }

    /// <summary>
    /// Custom implementor key.
    /// </summary>
    public IDependencyKey? ImplementorKey { get; }

    /// <summary>
    /// Creates a new <see cref="InjectableDependencyResolution{T}"/> instance with a custom factory.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired parameter or member, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="factory">Custom resolution factory.</param>
    /// <returns>New <see cref="InjectableDependencyResolution{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static InjectableDependencyResolution<T> FromFactory(Func<T, bool> predicate, Expression<Func<IDependencyScope, object>> factory)
    {
        return new InjectableDependencyResolution<T>( predicate, factory, null );
    }

    /// <summary>
    /// Creates a new <see cref="InjectableDependencyResolution{T}"/> instance with custom implementor type.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired parameter or member, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="implementorKey">Custom implementor key.</param>
    /// <returns>New <see cref="InjectableDependencyResolution{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static InjectableDependencyResolution<T> FromImplementorKey(Func<T, bool> predicate, IDependencyKey implementorKey)
    {
        return new InjectableDependencyResolution<T>( predicate, null, implementorKey );
    }
}
