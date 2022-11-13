using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

public static class DependencyBuilderExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyBuilder FromSharedImplementor<T>(this IDependencyBuilder builder)
    {
        return builder.FromSharedImplementor( typeof( T ) );
    }
}
