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
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlConstraintBuilderCollection : SqlConstraintBuilderCollection
{
    internal PostgreSqlConstraintBuilderCollection() { }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetPrimaryKey()" />
    [Pure]
    public new PostgreSqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.GetPrimaryKey() );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetPrimaryKey()" />
    [Pure]
    public new PostgreSqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.TryGetPrimaryKey() );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetIndex(string)" />
    [Pure]
    public new PostgreSqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public new PostgreSqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetCheck(string)" />
    [Pure]
    public new PostgreSqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public new PostgreSqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.SetPrimaryKey(SqlIndexBuilder)" />
    public PostgreSqlPrimaryKeyBuilder SetPrimaryKey(PostgreSqlIndexBuilder index)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.SetPrimaryKey( index ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.SetPrimaryKey(string,SqlIndexBuilder)" />
    public PostgreSqlPrimaryKeyBuilder SetPrimaryKey(string name, PostgreSqlIndexBuilder index)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.SetPrimaryKey( name, index ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateIndex(ReadOnlyArray{SqlOrderByNode},bool)" />
    public new PostgreSqlIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.CreateIndex( columns, isUnique ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateIndex(string,ReadOnlyArray{SqlOrderByNode},bool)" />
    public new PostgreSqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.CreateIndex( name, columns, isUnique ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateForeignKey(SqlIndexBuilder,SqlIndexBuilder)" />
    public PostgreSqlForeignKeyBuilder CreateForeignKey(PostgreSqlIndexBuilder originIndex, PostgreSqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateForeignKey(string,SqlIndexBuilder,SqlIndexBuilder)" />
    public PostgreSqlForeignKeyBuilder CreateForeignKey(
        string name,
        PostgreSqlIndexBuilder originIndex,
        PostgreSqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateCheck(SqlConditionNode)" />
    public new PostgreSqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.CreateCheck( condition ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateCheck(string,SqlConditionNode)" />
    public new PostgreSqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.CreateCheck( name, condition ) );
    }
}
