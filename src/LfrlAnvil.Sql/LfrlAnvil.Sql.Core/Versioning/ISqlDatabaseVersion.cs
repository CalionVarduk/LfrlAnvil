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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents a single database version.
/// </summary>
public interface ISqlDatabaseVersion
{
    /// <summary>
    /// Identifier of this version.
    /// </summary>
    Version Value { get; }

    /// <summary>
    /// Description of this version.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Applies changes defined by this version to the provided <paramref name="database"/>.
    /// </summary>
    /// <param name="database">Target SQL database builder.</param>
    void Apply(ISqlDatabaseBuilder database);
}
