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

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Specifies available automatic dependency implementor disposal strategies.
/// </summary>
public enum DependencyImplementorDisposalStrategyType : byte
{
    /// <summary>
    /// Invokes the <see cref="IDisposable.Dispose()"/> method if possible. This is the default strategy.
    /// </summary>
    UseDisposableInterface = 0,

    /// <summary>
    /// Invokes a custom callback.
    /// </summary>
    UseCallback = 1,

    /// <summary>
    /// Disables automatic disposal.
    /// </summary>
    RenounceOwnership = 2
}
