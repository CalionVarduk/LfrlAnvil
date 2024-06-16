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

namespace LfrlAnvil;

/// <summary>
/// Represents a result of an operation that can either add a new element or update an existing one.
/// </summary>
public enum AddOrUpdateResult : byte
{
    /// <summary>
    /// Specifies that an operation ended with addition of a new element.
    /// </summary>
    Added = 0,

    /// <summary>
    /// Specifies that an operation ended with update of an existing element.
    /// </summary>
    Updated = 1
}
