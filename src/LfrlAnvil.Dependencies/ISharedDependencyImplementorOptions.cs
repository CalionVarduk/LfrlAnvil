namespace LfrlAnvil.Dependencies;

public interface ISharedDependencyImplementorOptions
{
    ISharedDependencyImplementorKey Key { get; }

    void Keyed<TKey>(TKey key)
        where TKey : notnull;

    void NotKeyed();
}
