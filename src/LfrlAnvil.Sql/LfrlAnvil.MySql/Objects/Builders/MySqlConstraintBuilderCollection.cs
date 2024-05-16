using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlConstraintBuilderCollection : SqlConstraintBuilderCollection
{
    internal MySqlConstraintBuilderCollection() { }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetPrimaryKey()" />
    [Pure]
    public new MySqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.GetPrimaryKey() );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetPrimaryKey()" />
    [Pure]
    public new MySqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.TryGetPrimaryKey() );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetIndex(string)" />
    [Pure]
    public new MySqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetIndex(string)" />
    [Pure]
    public new MySqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.GetCheck(string)" />
    [Pure]
    public new MySqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.TryGetCheck(string)" />
    [Pure]
    public new MySqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.SetPrimaryKey(SqlIndexBuilder)" />
    public MySqlPrimaryKeyBuilder SetPrimaryKey(MySqlIndexBuilder index)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.SetPrimaryKey( index ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.SetPrimaryKey(string,SqlIndexBuilder)" />
    public MySqlPrimaryKeyBuilder SetPrimaryKey(string name, MySqlIndexBuilder index)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.SetPrimaryKey( name, index ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateIndex(ReadOnlyArray{SqlOrderByNode},bool)" />
    public new MySqlIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.CreateIndex( columns, isUnique ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateIndex(string,ReadOnlyArray{SqlOrderByNode},bool)" />
    public new MySqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.CreateIndex( name, columns, isUnique ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateForeignKey(SqlIndexBuilder,SqlIndexBuilder)" />
    public MySqlForeignKeyBuilder CreateForeignKey(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateForeignKey(string,SqlIndexBuilder,SqlIndexBuilder)" />
    public MySqlForeignKeyBuilder CreateForeignKey(string name, MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateCheck(SqlConditionNode)" />
    public new MySqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.CreateCheck( condition ) );
    }

    /// <inheritdoc cref="SqlConstraintBuilderCollection.CreateCheck(string,SqlConditionNode)" />
    public new MySqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.CreateCheck( name, condition ) );
    }
}
