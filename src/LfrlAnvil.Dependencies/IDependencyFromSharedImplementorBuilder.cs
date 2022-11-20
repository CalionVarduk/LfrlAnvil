namespace LfrlAnvil.Dependencies;

public interface IDependencyFromSharedImplementorBuilder : IDependencyBuilder
{
    IDependencyBuilder Keyed<TKey>(TKey key)
        where TKey : notnull;

    IDependencyBuilder NotKeyed();
}
