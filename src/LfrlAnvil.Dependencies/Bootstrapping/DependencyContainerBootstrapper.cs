// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
