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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlDefaultObjectNameProvider" />
public class SqlDefaultObjectNameProvider : ISqlDefaultObjectNameProvider
{
    /// <inheritdoc />
    [Pure]
    public virtual string GetForPrimaryKey(ISqlTableBuilder table)
    {
        return SqlHelpers.GetDefaultPrimaryKeyName( table );
    }

    /// <inheritdoc />
    [Pure]
    public virtual string GetForForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex)
    {
        return SqlHelpers.GetDefaultForeignKeyName( originIndex, referencedIndex );
    }

    /// <inheritdoc />
    [Pure]
    public virtual string GetForCheck(ISqlTableBuilder table)
    {
        return SqlHelpers.GetDefaultCheckName( table );
    }

    /// <inheritdoc />
    [Pure]
    public virtual string GetForIndex(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique)
    {
        return SqlHelpers.GetDefaultIndexName( table, columns, isUnique );
    }
}
