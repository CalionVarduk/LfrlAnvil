namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class SharedDependencyImplementorOptions : ISharedDependencyImplementorOptions
{
    private IInternalSharedDependencyImplementorKey _key;

    internal SharedDependencyImplementorOptions(IInternalSharedDependencyImplementorKey key)
    {
        _key = key;
    }

    public ISharedDependencyImplementorKey Key => _key;

    public void Keyed<TKey>(TKey key)
        where TKey : notnull
    {
        _key = new SharedDependencyImplementorKey<TKey>( _key.Type, key );
    }

    public void NotKeyed()
    {
        _key = new SharedDependencyImplementorKey( _key.Type );
    }
}
