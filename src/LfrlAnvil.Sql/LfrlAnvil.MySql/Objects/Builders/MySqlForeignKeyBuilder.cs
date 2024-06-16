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

using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal MySqlForeignKeyBuilder(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetName(string)" />
    public new MySqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetDefaultName()" />
    public new MySqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior)" />
    public new MySqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior)" />
    public new MySqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
