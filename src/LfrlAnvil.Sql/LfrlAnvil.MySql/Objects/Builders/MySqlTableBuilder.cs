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

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlTableBuilder : SqlTableBuilder
{
    internal MySqlTableBuilder(MySqlSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new MySqlColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new MySqlConstraintBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlTableBuilder.Schema" />
    public new MySqlSchemaBuilder Schema => ReinterpretCast.To<MySqlSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlTableBuilder.Columns" />
    public new MySqlColumnBuilderCollection Columns => ReinterpretCast.To<MySqlColumnBuilderCollection>( base.Columns );

    /// <inheritdoc cref="SqlTableBuilder.Constraints" />
    public new MySqlConstraintBuilderCollection Constraints => ReinterpretCast.To<MySqlConstraintBuilderCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTableBuilder.SetName(string)" />
    public new MySqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
