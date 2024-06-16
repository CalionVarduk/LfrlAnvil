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

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred during disposal of a resolved dependency instance owned by the disposed scope.
/// </summary>
public class OwnedDependencyDisposalException : Exception
{
    /// <summary>
    /// Creates a new <see cref="OwnedDependencyDisposalException"/> instance.
    /// </summary>
    /// <param name="scope">Disposed scope.</param>
    /// <param name="innerException">Disposal exception.</param>
    public OwnedDependencyDisposalException(IDependencyScope scope, Exception innerException)
        : base( Resources.OwnedDependencyHasThrownExceptionDuringDisposal( scope ), innerException )
    {
        Scope = scope;
    }

    /// <summary>
    /// Disposed scope.
    /// </summary>
    public IDependencyScope Scope { get; }
}
