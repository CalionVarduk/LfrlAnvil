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

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a strategy to use for extracting result set fields of a query.
/// </summary>
public enum SqlQueryReaderResultSetFieldsPersistenceMode : byte
{
    /// <summary>
    /// Specifies that result set fields should not be extracted at all.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Specifies that result set fields should be extracted but without information about field types.
    /// </summary>
    Persist = 1,

    /// <summary>
    /// Specifies that result set fields should be extracted, including field types.
    /// </summary>
    PersistWithTypes = 2
}
