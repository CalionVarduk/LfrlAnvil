using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyImplementorOptions : IDependencyImplementorOptions
{
    private IInternalDependencyImplementorKey _key;

    internal DependencyImplementorOptions(IInternalDependencyImplementorKey key)
    {
        _key = key;
    }

    public IDependencyImplementorKey Key => _key;

    public void Keyed<TKey>(TKey key)
        where TKey : notnull
    {
        _key = new DependencyImplementorKey<TKey>( _key.Type, key );
    }

    public void NotKeyed()
    {
        _key = new DependencyImplementorKey( _key.Type );
    }

    [Pure]
    internal static IInternalDependencyImplementorKey CreateImplementorKey(
        IInternalDependencyImplementorKey defaultKey,
        Action<IDependencyImplementorOptions>? configuration)
    {
        if ( configuration is null )
            return defaultKey;

        var options = new DependencyImplementorOptions( defaultKey );
        configuration( options );

        var sharedImplementorKey = options.Key as IInternalDependencyImplementorKey;
        Ensure.IsNotNull( sharedImplementorKey, nameof( sharedImplementorKey ) );
        return sharedImplementorKey;
    }
}
