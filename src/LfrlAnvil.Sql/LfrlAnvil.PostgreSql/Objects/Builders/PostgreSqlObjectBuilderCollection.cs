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

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlObjectBuilderCollection : SqlObjectBuilderCollection
{
    internal PostgreSqlObjectBuilderCollection() { }

    /// <inheritdoc cref="SqlObjectBuilderCollection.Schema" />
    public new PostgreSqlSchemaBuilder Schema => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetTable(string)" />
    [Pure]
    public new PostgreSqlTableBuilder GetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.GetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetTable(string)" />
    [Pure]
    public new PostgreSqlTableBuilder? TryGetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.TryGetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetIndex(string)" />
    [Pure]
    public new PostgreSqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public new PostgreSqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetPrimaryKey(string)" />
    [Pure]
    public new PostgreSqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.GetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public new PostgreSqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.TryGetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetCheck(string)" />
    [Pure]
    public new PostgreSqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public new PostgreSqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetView(string)" />
    [Pure]
    public new PostgreSqlViewBuilder GetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewBuilder>( base.GetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetView(string)" />
    [Pure]
    public new PostgreSqlViewBuilder? TryGetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewBuilder>( base.TryGetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.CreateTable(string)" />
    public new PostgreSqlTableBuilder CreateTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.CreateTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetOrCreateTable(string)" />
    public new PostgreSqlTableBuilder GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.GetOrCreateTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.CreateView(string,SqlQueryExpressionNode)" />
    public new PostgreSqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<PostgreSqlViewBuilder>( base.CreateView( name, source ) );
    }

    /// <inheritdoc />
    protected override PostgreSqlTableBuilder CreateTableBuilder(string name)
    {
        return new PostgreSqlTableBuilder( Schema, name );
    }

    /// <inheritdoc />
    protected override PostgreSqlViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new PostgreSqlViewBuilder( Schema, name, source, referencedObjects );
    }

    /// <inheritdoc />
    protected override PostgreSqlIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new PostgreSqlIndexBuilder(
            ReinterpretCast.To<PostgreSqlTableBuilder>( table ),
            name,
            new SqlIndexBuilderColumns<PostgreSqlColumnBuilder>( columns.Expressions ),
            isUnique,
            referencedColumns );
    }

    /// <inheritdoc />
    protected override PostgreSqlPrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new PostgreSqlPrimaryKeyBuilder( ReinterpretCast.To<PostgreSqlIndexBuilder>( index ), name );
    }

    /// <inheritdoc />
    protected override PostgreSqlForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new PostgreSqlForeignKeyBuilder(
            ReinterpretCast.To<PostgreSqlIndexBuilder>( originIndex ),
            ReinterpretCast.To<PostgreSqlIndexBuilder>( referencedIndex ),
            name );
    }

    /// <inheritdoc />
    protected override PostgreSqlCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new PostgreSqlCheckBuilder( ReinterpretCast.To<PostgreSqlTableBuilder>( table ), name, condition, referencedColumns );
    }
}
