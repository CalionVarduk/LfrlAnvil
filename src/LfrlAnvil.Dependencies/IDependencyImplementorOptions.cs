namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorOptions
{
    IDependencyKey Key { get; }

    void Keyed<TKey>(TKey key)
        where TKey : notnull;

    void NotKeyed();
}
