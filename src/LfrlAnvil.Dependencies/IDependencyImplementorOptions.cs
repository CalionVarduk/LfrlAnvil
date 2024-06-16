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
