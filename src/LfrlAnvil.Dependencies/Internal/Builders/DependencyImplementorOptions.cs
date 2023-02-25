using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyImplementorOptions : IDependencyImplementorOptions
{
    private IInternalDependencyKey _key;

    internal DependencyImplementorOptions(IInternalDependencyKey key)
    {
        _key = key;
    }

    public IDependencyKey Key => _key;

    public void Keyed<TKey>(TKey key)
        where TKey : notnull
    {
        _key = new DependencyKey<TKey>( _key.Type, key );
    }

    public void NotKeyed()
    {
        _key = new DependencyKey( _key.Type );
    }

    [Pure]
    internal static IInternalDependencyKey CreateImplementorKey(
        IInternalDependencyKey defaultKey,
        Action<IDependencyImplementorOptions>? configuration)
    {
        if ( configuration is null )
            return defaultKey;

        var options = new DependencyImplementorOptions( defaultKey );
        configuration( options );

        var sharedImplementorKey = options.Key as IInternalDependencyKey;
        Ensure.IsNotNull( sharedImplementorKey, nameof( sharedImplementorKey ) );
        return sharedImplementorKey;
    }
}
