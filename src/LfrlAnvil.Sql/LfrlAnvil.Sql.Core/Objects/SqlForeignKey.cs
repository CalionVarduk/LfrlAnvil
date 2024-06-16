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

using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlForeignKey" />
public abstract class SqlForeignKey : SqlConstraint, ISqlForeignKey
{
    /// <summary>
    /// Creates a new <see cref="SqlForeignKey"/> instance.
    /// </summary>
    /// <param name="originIndex">SQL index that this foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by this foreign key.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
        : base( originIndex.Table, builder )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = builder.OnDeleteBehavior;
        OnUpdateBehavior = builder.OnUpdateBehavior;
    }

    /// <inheritdoc cref="ISqlForeignKey.OriginIndex" />
    public SqlIndex OriginIndex { get; }

    /// <inheritdoc cref="ISqlForeignKey.ReferencedIndex" />
    public SqlIndex ReferencedIndex { get; }

    /// <inheritdoc />
    public ReferenceBehavior OnDeleteBehavior { get; }

    /// <inheritdoc />
    public ReferenceBehavior OnUpdateBehavior { get; }

    ISqlIndex ISqlForeignKey.OriginIndex => OriginIndex;
    ISqlIndex ISqlForeignKey.ReferencedIndex => ReferencedIndex;
}
