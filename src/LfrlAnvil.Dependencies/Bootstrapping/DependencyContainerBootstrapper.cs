using System;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Bootstrapping;

public abstract class DependencyContainerBootstrapper : IDependencyContainerBootstrapper<DependencyContainerBuilder>
{
    private InterlockedBoolean _inProgress;

    protected DependencyContainerBootstrapper()
    {
        _inProgress = new InterlockedBoolean( false );
    }

    public void Bootstrap(DependencyContainerBuilder builder)
    {
        if ( ! _inProgress.WriteTrue() )
            throw new InvalidOperationException( Resources.BootstrapperInvokedBeforeItCouldFinish );

        BootstrapCore( builder );
        _inProgress.WriteFalse();
    }

    protected abstract void BootstrapCore(DependencyContainerBuilder builder);
}
