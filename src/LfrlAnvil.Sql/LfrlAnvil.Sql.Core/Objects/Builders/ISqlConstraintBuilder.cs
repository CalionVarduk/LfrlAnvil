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

using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL constraint builder attached to a table.
/// </summary>
public interface ISqlConstraintBuilder : ISqlObjectBuilder
{
    /// <summary>
    /// Table that this constraint is attached to.
    /// </summary>
    ISqlTableBuilder Table { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlConstraintBuilder SetName(string name);

    /// <summary>
    /// Changes the name of this object to a default name.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When name cannot be changed.</exception>
    /// <remarks>See <see cref="ISqlDefaultObjectNameProvider"/> for more information.</remarks>
    ISqlConstraintBuilder SetDefaultName();
}
