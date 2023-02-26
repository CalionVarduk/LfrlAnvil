using System;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

public static class DependencyBuilderExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyBuilder FromSharedImplementor<T>(
        this IDependencyBuilder builder,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        return builder.FromSharedImplementor( typeof( T ), configuration );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyImplementorBuilder FromType<T>(
        this IDependencyBuilder builder,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return builder.FromType( typeof( T ), configuration );
    }
}
