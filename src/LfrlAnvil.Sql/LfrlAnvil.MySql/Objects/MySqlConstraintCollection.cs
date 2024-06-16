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
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlConstraintCollection : SqlConstraintCollection
{
    internal MySqlConstraintCollection(MySqlConstraintBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlConstraintCollection.PrimaryKey" />
    public new MySqlPrimaryKey PrimaryKey => ReinterpretCast.To<MySqlPrimaryKey>( base.PrimaryKey );

    /// <inheritdoc cref="SqlConstraintCollection.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlConstraintCollection.GetIndex(string)" />
    [Pure]
    public new MySqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetIndex(string)" />
    [Pure]
    public new MySqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetCheck(string)" />
    [Pure]
    public new MySqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetCheck(string)" />
    [Pure]
    public new MySqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.TryGetCheck( name ) );
    }
}
