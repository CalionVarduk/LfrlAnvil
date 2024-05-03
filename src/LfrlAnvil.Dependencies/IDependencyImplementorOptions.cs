namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents available options for dependency implementors.
/// </summary>
public interface IDependencyImplementorOptions
{
    /// <summary>
    /// Dependency's identifier.
    /// </summary>
    IDependencyKey Key { get; }

    /// <summary>
    /// Changes the <see cref="Key"/> to use keyed locators.
    /// </summary>
    /// <param name="key">Locator's key.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    void Keyed<TKey>(TKey key)
        where TKey : notnull;

    /// <summary>
    /// Changes the <see cref="Key"/> to not use keyed locators.
    /// </summary>
    void NotKeyed();
}
