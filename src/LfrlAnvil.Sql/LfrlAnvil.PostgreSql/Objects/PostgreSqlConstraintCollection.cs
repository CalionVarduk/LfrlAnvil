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
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlConstraintCollection : SqlConstraintCollection
{
    internal PostgreSqlConstraintCollection(PostgreSqlConstraintBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlConstraintCollection.PrimaryKey" />
    public new PostgreSqlPrimaryKey PrimaryKey => ReinterpretCast.To<PostgreSqlPrimaryKey>( base.PrimaryKey );

    /// <inheritdoc cref="SqlConstraintCollection.Table" />
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    /// <inheritdoc cref="SqlConstraintCollection.GetIndex(string)" />
    [Pure]
    public new PostgreSqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetIndex(string)" />
    [Pure]
    public new PostgreSqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetCheck(string)" />
    [Pure]
    public new PostgreSqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetCheck(string)" />
    [Pure]
    public new PostgreSqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.TryGetCheck( name ) );
    }
}
