using System;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

/// <summary>
/// Contains <see cref="IDependencyImplementorBuilder"/> extension methods.
/// </summary>
public static class DependencyImplementorBuilderExtensions
{
    /// <summary>
    /// Specifies that this implementor's instances should be created
    /// by the best suited constructor of the provided type.
    /// </summary>
    /// <param name="builder">Source dependency implementor builder.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <typeparam name="T">Implementor's type.</typeparam>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyImplementorBuilder FromType<T>(
        this IDependencyImplementorBuilder builder,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return builder.FromType( typeof( T ), configuration );
    }
}
