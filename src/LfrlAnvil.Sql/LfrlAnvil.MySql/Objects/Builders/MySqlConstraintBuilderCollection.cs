using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlConstraintBuilderCollection : SqlConstraintBuilderCollection
{
    internal MySqlConstraintBuilderCollection() { }

    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    [Pure]
    public new MySqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.GetPrimaryKey() );
    }

    [Pure]
    public new MySqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.TryGetPrimaryKey() );
    }

    [Pure]
    public new MySqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.GetIndex( name ) );
    }

    [Pure]
    public new MySqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new MySqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new MySqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new MySqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.GetCheck( name ) );
    }

    [Pure]
    public new MySqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    public MySqlPrimaryKeyBuilder SetPrimaryKey(MySqlIndexBuilder index)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.SetPrimaryKey( index ) );
    }

    public MySqlPrimaryKeyBuilder SetPrimaryKey(string name, MySqlIndexBuilder index)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.SetPrimaryKey( name, index ) );
    }

    public new MySqlIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.CreateIndex( columns, isUnique ) );
    }

    public new MySqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.CreateIndex( name, columns, isUnique ) );
    }

    public MySqlForeignKeyBuilder CreateForeignKey(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    public MySqlForeignKeyBuilder CreateForeignKey(string name, MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    public new MySqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.CreateCheck( condition ) );
    }

    public new MySqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.CreateCheck( name, condition ) );
    }
}
