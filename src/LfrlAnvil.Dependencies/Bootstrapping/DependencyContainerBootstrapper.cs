using System;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Bootstrapping;

/// <summary>
/// Represents a <see cref="DependencyContainerBuilder"/> bootstrapper that contains a set of dependency definitions.
/// </summary>
public abstract class DependencyContainerBootstrapper : IDependencyContainerBootstrapper<DependencyContainerBuilder>
{
    private InterlockedBoolean _inProgress;

    /// <summary>
    /// Creates a new <see cref="DependencyContainerBootstrapper"/> instance.
    /// </summary>
    protected DependencyContainerBootstrapper()
    {
        _inProgress = new InterlockedBoolean( false );
    }

    /// <inheritdoc />
    public void Bootstrap(DependencyContainerBuilder builder)
    {
        if ( ! _inProgress.WriteTrue() )
            throw new InvalidOperationException( Resources.BootstrapperInvokedBeforeItCouldFinish );

        BootstrapCore( builder );
        _inProgress.WriteFalse();
    }

    /// <summary>
    /// Provides an implementation of populating the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Builder to populate.</param>
    protected abstract void BootstrapCore(DependencyContainerBuilder builder);
}
