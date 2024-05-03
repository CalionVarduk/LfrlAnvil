using System;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

/// <summary>
/// Contains <see cref="IDependencyBuilder"/> extension methods.
/// </summary>
public static class DependencyBuilderExtensions
{
    /// <summary>
    /// Specifies that this dependency should be implemented through a shared implementor of the provided type.
    /// </summary>
    /// <param name="builder">Source dependency builder.</param>
    /// <param name="configuration">Optional configurator of the shared implementor.</param>
    /// <typeparam name="T">Shared implementor's type.</typeparam>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>Resets <see cref="IDependencyBuilder.Implementor"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyBuilder FromSharedImplementor<T>(
        this IDependencyBuilder builder,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        return builder.FromSharedImplementor( typeof( T ), configuration );
    }

    /// <summary>
    /// Specifies that this dependency should be implemented by the best suited constructor of the provided type.
    /// </summary>
    /// <param name="builder">Source dependency builder.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <typeparam name="T">Implementor's type.</typeparam>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>Resets <see cref="IDependencyBuilder.SharedImplementorKey"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyImplementorBuilder FromType<T>(
        this IDependencyBuilder builder,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return builder.FromType( typeof( T ), configuration );
    }
}
