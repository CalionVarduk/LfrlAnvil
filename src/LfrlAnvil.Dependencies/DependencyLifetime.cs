namespace LfrlAnvil.Dependencies;

public enum DependencyLifetime : byte
{
    Transient = 0,
    Scoped = 1,
    ScopedSingleton = 2,
    Singleton = 3
}
