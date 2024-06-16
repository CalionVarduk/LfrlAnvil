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

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlSchemaBuilder : SqlSchemaBuilder
{
    internal PostgreSqlSchemaBuilder(PostgreSqlDatabaseBuilder database, string name)
        : base( database, name, new PostgreSqlObjectBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilder.Objects" />
    public new PostgreSqlObjectBuilderCollection Objects => ReinterpretCast.To<PostgreSqlObjectBuilderCollection>( base.Objects );

    /// <inheritdoc cref="SqlSchemaBuilder.SetName(string)" />
    public new PostgreSqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
