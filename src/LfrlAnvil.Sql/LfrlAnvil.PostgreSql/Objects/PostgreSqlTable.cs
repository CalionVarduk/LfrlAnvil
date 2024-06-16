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

using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlTable : SqlTable
{
    internal PostgreSqlTable(PostgreSqlSchema schema, PostgreSqlTableBuilder builder)
        : base(
            schema,
            builder,
            new PostgreSqlColumnCollection( builder.Columns ),
            new PostgreSqlConstraintCollection( builder.Constraints ) ) { }

    /// <inheritdoc cref="SqlTable.Columns" />
    public new PostgreSqlColumnCollection Columns => ReinterpretCast.To<PostgreSqlColumnCollection>( base.Columns );

    /// <inheritdoc cref="SqlTable.Constraints" />
    public new PostgreSqlConstraintCollection Constraints => ReinterpretCast.To<PostgreSqlConstraintCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTable.Schema" />
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
