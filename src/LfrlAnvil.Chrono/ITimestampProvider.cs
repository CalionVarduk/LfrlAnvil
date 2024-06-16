﻿// Copyright 2024 Łukasz Furlepa
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

using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a provider of <see cref="Timestamp"/> instances.
/// </summary>
public interface ITimestampProvider : IGenerator<Timestamp>
{
    /// <summary>
    /// Returns the current <see cref="Timestamp"/>.
    /// </summary>
    /// <returns>Current <see cref="Timestamp"/>.</returns>
    Timestamp GetNow();
}
