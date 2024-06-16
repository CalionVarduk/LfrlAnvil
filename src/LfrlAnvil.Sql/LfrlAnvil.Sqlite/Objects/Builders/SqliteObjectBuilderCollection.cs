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
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteObjectBuilderCollection : SqlObjectBuilderCollection
{
    internal SqliteObjectBuilderCollection() { }

    /// <inheritdoc cref="SqlObjectBuilderCollection.Schema" />
    public new SqliteSchemaBuilder Schema => ReinterpretCast.To<SqliteSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetTable(string)" />
    [Pure]
    public new SqliteTableBuilder GetTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.GetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetTable(string)" />
    [Pure]
    public new SqliteTableBuilder? TryGetTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.TryGetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetIndex(string)" />
    [Pure]
    public new SqliteIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public new SqliteIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetPrimaryKey(string)" />
    [Pure]
    public new SqlitePrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.GetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public new SqlitePrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.TryGetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetCheck(string)" />
    [Pure]
    public new SqliteCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public new SqliteCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetView(string)" />
    [Pure]
    public new SqliteViewBuilder GetView(string name)
    {
        return ReinterpretCast.To<SqliteViewBuilder>( base.GetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetView(string)" />
    [Pure]
    public new SqliteViewBuilder? TryGetView(string name)
    {
        return ReinterpretCast.To<SqliteViewBuilder>( base.TryGetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.CreateTable(string)" />
    public new SqliteTableBuilder CreateTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.CreateTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetOrCreateTable(string)" />
    public new SqliteTableBuilder GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.GetOrCreateTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.CreateView(string,SqlQueryExpressionNode)" />
    public new SqliteViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<SqliteViewBuilder>( base.CreateView( name, source ) );
    }

    /// <inheritdoc />
    protected override SqliteTableBuilder CreateTableBuilder(string name)
    {
        return new SqliteTableBuilder( Schema, name );
    }

    /// <inheritdoc />
    protected override SqliteViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new SqliteViewBuilder( Schema, name, source, referencedObjects );
    }

    /// <inheritdoc />
    protected override SqliteIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new SqliteIndexBuilder(
            ReinterpretCast.To<SqliteTableBuilder>( table ),
            name,
            new SqlIndexBuilderColumns<SqliteColumnBuilder>( columns.Expressions ),
            isUnique,
            referencedColumns );
    }

    /// <inheritdoc />
    protected override SqlitePrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new SqlitePrimaryKeyBuilder( ReinterpretCast.To<SqliteIndexBuilder>( index ), name );
    }

    /// <inheritdoc />
    protected override SqliteForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new SqliteForeignKeyBuilder(
            ReinterpretCast.To<SqliteIndexBuilder>( originIndex ),
            ReinterpretCast.To<SqliteIndexBuilder>( referencedIndex ),
            name );
    }

    /// <inheritdoc />
    protected override SqliteCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new SqliteCheckBuilder( ReinterpretCast.To<SqliteTableBuilder>( table ), name, condition, referencedColumns );
    }
}
