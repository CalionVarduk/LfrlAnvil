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

using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL check constraint builder.
/// </summary>
public interface ISqlCheckBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// Underlying condition of this check constraint.
    /// </summary>
    SqlConditionNode Condition { get; }

    /// <summary>
    /// Collection of columns referenced by this check constraint.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedColumns { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlCheckBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlCheckBuilder SetDefaultName();
}
