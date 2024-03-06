using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteConstraintBuilderCollection : SqlConstraintBuilderCollection
{
    internal SqliteConstraintBuilderCollection() { }

    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    [Pure]
    public new SqlitePrimaryKeyBuilder GetPrimaryKey()
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.GetPrimaryKey() );
    }

    [Pure]
    public new SqlitePrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.TryGetPrimaryKey() );
    }

    [Pure]
    public new SqliteIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.GetIndex( name ) );
    }

    [Pure]
    public new SqliteIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new SqliteForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new SqliteForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new SqliteCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.GetCheck( name ) );
    }

    [Pure]
    public new SqliteCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.TryGetCheck( name ) );
    }

    public SqlitePrimaryKeyBuilder SetPrimaryKey(SqliteIndexBuilder index)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.SetPrimaryKey( index ) );
    }

    public SqlitePrimaryKeyBuilder SetPrimaryKey(string name, SqliteIndexBuilder index)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.SetPrimaryKey( name, index ) );
    }

    public new SqliteIndexBuilder CreateIndex(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.CreateIndex( columns, isUnique ) );
    }

    public new SqliteIndexBuilder CreateIndex(
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique = false)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.CreateIndex( name, columns, isUnique ) );
    }

    public SqliteForeignKeyBuilder CreateForeignKey(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    public SqliteForeignKeyBuilder CreateForeignKey(string name, SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    public new SqliteCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.CreateCheck( condition ) );
    }

    public new SqliteCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.CreateCheck( name, condition ) );
    }
}
