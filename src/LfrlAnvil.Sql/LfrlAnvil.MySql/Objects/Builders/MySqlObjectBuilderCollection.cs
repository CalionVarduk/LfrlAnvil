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

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlObjectBuilderCollection : SqlObjectBuilderCollection
{
    internal MySqlObjectBuilderCollection() { }

    /// <inheritdoc cref="SqlObjectBuilderCollection.Schema" />
    public new MySqlSchemaBuilder Schema => ReinterpretCast.To<MySqlSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetTable(string)" />
    [Pure]
    public new MySqlTableBuilder GetTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.GetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetTable(string)" />
    [Pure]
    public new MySqlTableBuilder? TryGetTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.TryGetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetIndex(string)" />
    [Pure]
    public new MySqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public new MySqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetPrimaryKey(string)" />
    [Pure]
    public new MySqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.GetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public new MySqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.TryGetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetCheck(string)" />
    [Pure]
    public new MySqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public new MySqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetView(string)" />
    [Pure]
    public new MySqlViewBuilder GetView(string name)
    {
        return ReinterpretCast.To<MySqlViewBuilder>( base.GetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.TryGetView(string)" />
    [Pure]
    public new MySqlViewBuilder? TryGetView(string name)
    {
        return ReinterpretCast.To<MySqlViewBuilder>( base.TryGetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.CreateTable(string)" />
    public new MySqlTableBuilder CreateTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.CreateTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.GetOrCreateTable(string)" />
    public new MySqlTableBuilder GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.GetOrCreateTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectBuilderCollection.CreateView(string,SqlQueryExpressionNode)" />
    public new MySqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<MySqlViewBuilder>( base.CreateView( name, source ) );
    }

    /// <inheritdoc />
    protected override MySqlTableBuilder CreateTableBuilder(string name)
    {
        return new MySqlTableBuilder( Schema, name );
    }

    /// <inheritdoc />
    protected override MySqlViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new MySqlViewBuilder( Schema, name, source, referencedObjects );
    }

    /// <inheritdoc />
    protected override MySqlIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new MySqlIndexBuilder(
            ReinterpretCast.To<MySqlTableBuilder>( table ),
            name,
            new SqlIndexBuilderColumns<MySqlColumnBuilder>( columns.Expressions ),
            isUnique,
            referencedColumns );
    }

    /// <inheritdoc />
    protected override MySqlPrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new MySqlPrimaryKeyBuilder( ReinterpretCast.To<MySqlIndexBuilder>( index ), name );
    }

    /// <inheritdoc />
    protected override MySqlForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new MySqlForeignKeyBuilder(
            ReinterpretCast.To<MySqlIndexBuilder>( originIndex ),
            ReinterpretCast.To<MySqlIndexBuilder>( referencedIndex ),
            name );
    }

    /// <inheritdoc />
    protected override MySqlCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new MySqlCheckBuilder( ReinterpretCast.To<MySqlTableBuilder>( table ), name, condition, referencedColumns );
    }
}
