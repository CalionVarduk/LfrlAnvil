namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorOptions
{
    IDependencyImplementorKey Key { get; }

    void Keyed<TKey>(TKey key)
        where TKey : notnull;

    void NotKeyed();
}
