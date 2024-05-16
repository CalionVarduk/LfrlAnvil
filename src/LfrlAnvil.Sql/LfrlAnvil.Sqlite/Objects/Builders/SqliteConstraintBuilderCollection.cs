using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteConstraintBuilderCollection : SqlConstraintBuilderCollection
{
    internal SqliteConstraintBuilderCollection() { }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetPrimaryKey()" />
    [Pure]
    public new SqlitePrimaryKeyBuilder GetPrimaryKey()
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.GetPrimaryKey() );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetPrimaryKey()" />
    [Pure]
    public new SqlitePrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.TryGetPrimaryKey() );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetIndex(string)" />
    [Pure]
    public new SqliteIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public new SqliteIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public new SqliteForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetCheck(string)" />
    [Pure]
    public new SqliteCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public new SqliteCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.SetPrimaryKey(SqlIndexBuilder)" />
    public SqlitePrimaryKeyBuilder SetPrimaryKey(SqliteIndexBuilder index)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.SetPrimaryKey( index ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.SetPrimaryKey(string,SqlIndexBuilder)" />
    public SqlitePrimaryKeyBuilder SetPrimaryKey(string name, SqliteIndexBuilder index)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.SetPrimaryKey( name, index ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateIndex(ReadOnlyArray{SqlOrderByNode},bool)" />
    public new SqliteIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.CreateIndex( columns, isUnique ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateIndex(string,ReadOnlyArray{SqlOrderByNode},bool)" />
    public new SqliteIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.CreateIndex( name, columns, isUnique ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateForeignKey(SqlIndexBuilder,SqlIndexBuilder)" />
    public SqliteForeignKeyBuilder CreateForeignKey(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateForeignKey(string,SqlIndexBuilder,SqlIndexBuilder)" />
    public SqliteForeignKeyBuilder CreateForeignKey(string name, SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateCheck(SqlConditionNode)" />
    public new SqliteCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.CreateCheck( condition ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateCheck(string,SqlConditionNode)" />
    public new SqliteCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.CreateCheck( name, condition ) );
    }
}
