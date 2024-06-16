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

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlPrimaryKeyBuilder" />
public abstract class SqlPrimaryKeyBuilder : SqlConstraintBuilder, ISqlPrimaryKeyBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKeyBuilder"/> instance.
    /// </summary>
    /// <param name="index">Underlying index that defines this primary key.</param>
    /// <param name="name">Object's name.</param>
    protected SqlPrimaryKeyBuilder(SqlIndexBuilder index, string name)
        : base( index.Table, SqlObjectType.PrimaryKey, name )
    {
        Index = index;
    }

    /// <inheritdoc cref="ISqlPrimaryKeyBuilder.Index" />
    public SqlIndexBuilder Index { get; }

    /// <inheritdoc />
    public override bool CanRemove => Index.CanRemove;

    ISqlIndexBuilder ISqlPrimaryKeyBuilder.Index => Index;

    /// <inheritdoc cref="SqlConstraintBuilder.SetName(string)" />
    public new SqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlConstraintBuilder.SetDefaultName()" />
    public new SqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForPrimaryKey( Table );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        Index.Remove();
    }

    /// <inheritdoc />
    protected override void AfterRemove() { }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
