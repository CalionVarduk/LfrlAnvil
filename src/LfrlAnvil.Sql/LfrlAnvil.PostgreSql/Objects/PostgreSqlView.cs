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
public sealed class PostgreSqlView : SqlView
{
    internal PostgreSqlView(PostgreSqlSchema schema, PostgreSqlViewBuilder builder)
        : base( schema, builder, new PostgreSqlViewDataFieldCollection( builder.Source ) ) { }

    /// <inheritdoc cref="SqlView.DataFields" />
    public new PostgreSqlViewDataFieldCollection DataFields => ReinterpretCast.To<PostgreSqlViewDataFieldCollection>( base.DataFields );

    /// <inheritdoc cref="SqlView.Schema" />
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
