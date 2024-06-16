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
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteObjectCollection : SqlObjectCollection
{
    internal SqliteObjectCollection(SqliteObjectBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlObjectCollection.Schema" />
    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );

    /// <inheritdoc cref="SqlObjectCollection.GetTable(string)" />
    [Pure]
    public new SqliteTable GetTable(string name)
    {
        return ReinterpretCast.To<SqliteTable>( base.GetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetTable(string)" />
    [Pure]
    public new SqliteTable? TryGetTable(string name)
    {
        return ReinterpretCast.To<SqliteTable>( base.TryGetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetIndex(string)" />
    [Pure]
    public new SqliteIndex GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetIndex(string)" />
    [Pure]
    public new SqliteIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetPrimaryKey(string)" />
    [Pure]
    public new SqlitePrimaryKey GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKey>( base.GetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public new SqlitePrimaryKey? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKey>( base.TryGetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetCheck(string)" />
    [Pure]
    public new SqliteCheck GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetCheck(string)" />
    [Pure]
    public new SqliteCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetView(string)" />
    [Pure]
    public new SqliteView GetView(string name)
    {
        return ReinterpretCast.To<SqliteView>( base.GetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetView(string)" />
    [Pure]
    public new SqliteView? TryGetView(string name)
    {
        return ReinterpretCast.To<SqliteView>( base.TryGetView( name ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteTable CreateTable(SqlTableBuilder builder)
    {
        return new SqliteTable( Schema, ReinterpretCast.To<SqliteTableBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteView CreateView(SqlViewBuilder builder)
    {
        return new SqliteView( Schema, ReinterpretCast.To<SqliteViewBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteIndex CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new SqliteIndex( ReinterpretCast.To<SqliteTable>( table ), ReinterpretCast.To<SqliteIndexBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqlitePrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new SqlitePrimaryKey( ReinterpretCast.To<SqliteIndex>( index ), ReinterpretCast.To<SqlitePrimaryKeyBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteCheck CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new SqliteCheck( ReinterpretCast.To<SqliteTable>( table ), ReinterpretCast.To<SqliteCheckBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new SqliteForeignKey(
            ReinterpretCast.To<SqliteIndex>( originIndex ),
            ReinterpretCast.To<SqliteIndex>( referencedIndex ),
            ReinterpretCast.To<SqliteForeignKeyBuilder>( builder ) );
    }
}
