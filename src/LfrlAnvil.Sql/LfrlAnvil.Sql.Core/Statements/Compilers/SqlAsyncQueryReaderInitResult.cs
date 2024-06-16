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
/// Represents a result of an action that prepares a compiled asynchronous query reader for reading rows.
/// </summary>
/// <param name="Ordinals">Collection of field ordinals.</param>
/// <param name="Fields">Collection of definitions of row fields.</param>
public readonly record struct SqlAsyncQueryReaderInitResult(int[] Ordinals, SqlResultSetField[]? Fields);
