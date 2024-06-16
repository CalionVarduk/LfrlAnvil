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

using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Identifiers;

/// <summary>
/// Represents a generator of <see cref="Identifier"/> instances.
/// </summary>
public interface IIdentifierGenerator : IGenerator<Identifier>
{
    /// <summary>
    /// <see cref="Timestamp"/> of the first possible <see cref="Identifier"/> created by this generator.
    /// </summary>
    Timestamp BaseTimestamp { get; }

    /// <summary>
    /// Extracts a <see cref="Timestamp"/> used to create the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id"><see cref="Identifier"/> to extract <see cref="Timestamp"/> from.</param>
    /// <returns>New <see cref="Timestamp"/> instance.</returns>
    [Pure]
    Timestamp GetTimestamp(Identifier id);
}
