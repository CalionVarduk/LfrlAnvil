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

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL foreign key constraint.
/// </summary>
public interface ISqlForeignKey : ISqlConstraint
{
    /// <summary>
    /// SQL index referenced by this foreign key.
    /// </summary>
    ISqlIndex ReferencedIndex { get; }

    /// <summary>
    /// SQL index that this foreign key originates from.
    /// </summary>
    ISqlIndex OriginIndex { get; }

    /// <summary>
    /// Specifies this foreign key's on delete behavior.
    /// </summary>
    ReferenceBehavior OnDeleteBehavior { get; }

    /// <summary>
    /// Specifies this foreign key's on update behavior.
    /// </summary>
    ReferenceBehavior OnUpdateBehavior { get; }
}
