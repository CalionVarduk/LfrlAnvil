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
/// Represents an SQL object builder.
/// </summary>
public interface ISqlObjectBuilder
{
    /// <summary>
    /// Object's name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Object's type.
    /// </summary>
    SqlObjectType Type { get; }

    /// <summary>
    /// Database that this object belongs to.
    /// </summary>
    ISqlDatabaseBuilder Database { get; }

    /// <summary>
    /// Collection of sources that reference this object.
    /// </summary>
    SqlObjectBuilderReferenceCollection<ISqlObjectBuilder> ReferencingObjects { get; }

    /// <summary>
    /// Specifies whether or not this object has been removed.
    /// </summary>
    bool IsRemoved { get; }

    /// <summary>
    /// Specifies whether or not this object can be removed.
    /// </summary>
    bool CanRemove { get; }

    /// <summary>
    /// Changes the name of this object.
    /// </summary>
    /// <param name="name">Name to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When name cannot be changed.</exception>
    ISqlObjectBuilder SetName(string name);

    /// <summary>
    /// Removes this object.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When this object cannot be removed.</exception>
    void Remove();
}
