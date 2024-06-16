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
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteConstraintCollection : SqlConstraintCollection
{
    internal SqliteConstraintCollection(SqliteConstraintBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlConstraintCollection.PrimaryKey" />
    public new SqlitePrimaryKey PrimaryKey => ReinterpretCast.To<SqlitePrimaryKey>( base.PrimaryKey );

    /// <inheritdoc cref="SqlConstraintCollection.Table" />
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    /// <inheritdoc cref="SqlConstraintCollection.GetIndex(string)" />
    [Pure]
    public new SqliteIndex GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetIndex(string)" />
    [Pure]
    public new SqliteIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetCheck(string)" />
    [Pure]
    public new SqliteCheck GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetCheck(string)" />
    [Pure]
    public new SqliteCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.TryGetCheck( name ) );
    }
}
