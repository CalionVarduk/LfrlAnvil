// Copyright 2026 Łukasz Furlepa
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

using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an update part of the <see cref="SqlUpsertNode"/>.
/// </summary>
/// <param name="Assignments">Collection of value assignments that the update part of the upsert refers to.</param>
/// <param name="Filter"> Optional filter for rows to update.</param>
public readonly record struct SqlUpsertNodeUpdatePart(IEnumerable<SqlValueAssignmentNode> Assignments, SqlConditionNode? Filter = null);
