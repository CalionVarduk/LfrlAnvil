using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

public static class DependencyLocatorBuilderExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyImplementorBuilder AddSharedImplementor<T>(this IDependencyLocatorBuilder builder)
    {
        return builder.AddSharedImplementor( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyBuilder Add<T>(this IDependencyLocatorBuilder builder)
    {
        return builder.Add( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyRangeBuilder GetDependencyRange<T>(this IDependencyLocatorBuilder builder)
    {
        return builder.GetDependencyRange( typeof( T ) );
    }
}
