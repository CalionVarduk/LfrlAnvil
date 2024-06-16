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
public sealed class MySqlPrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal MySqlPrimaryKeyBuilder(MySqlIndexBuilder index, string name)
        : base( index, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.Index" />
    public new MySqlIndexBuilder Index => ReinterpretCast.To<MySqlIndexBuilder>( base.Index );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetName(string)" />
    public new MySqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetDefaultName()" />
    public new MySqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
