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

using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlIndex : SqlIndex
{
    internal MySqlIndex(MySqlTable table, MySqlIndexBuilder builder)
        : base( table, builder ) { }

    /// <inheritdoc cref="SqlIndex.Columns" />
    public new SqlIndexedArray<MySqlColumn> Columns => base.Columns.UnsafeReinterpretAs<MySqlColumn>();

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
