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

using System.Data;
using System.Data.Common;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an event that occurred due to a DB connection's state change.
/// </summary>
/// <param name="Connection">DB connection whose state has changed.</param>
/// <param name="StateChange">Underlying <see cref="StateChangeEventArgs"/> instance.</param>
public readonly record struct SqlDatabaseConnectionChangeEvent(DbConnection Connection, StateChangeEventArgs StateChange);
