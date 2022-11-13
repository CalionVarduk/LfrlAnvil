using System;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyDisposer
{
    internal readonly IDisposable Dependency;

    internal DependencyDisposer(IDisposable dependency)
    {
        Dependency = dependency;
    }

    internal Exception? TryDispose()
    {
        try
        {
            Dependency.Dispose();
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return null;
    }
}
