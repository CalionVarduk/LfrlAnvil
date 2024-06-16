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

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a state of existence of an SQL object builder instance.
/// </summary>
public enum SqlObjectExistenceState : byte
{
    /// <summary>
    /// Specifies that an object has not been created or removed.
    /// </summary>
    Unchanged = 0,

    /// <summary>
    /// Specifies that an object has been created.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Specifies that an object has been removed.
    /// </summary>
    Removed = 2
}
