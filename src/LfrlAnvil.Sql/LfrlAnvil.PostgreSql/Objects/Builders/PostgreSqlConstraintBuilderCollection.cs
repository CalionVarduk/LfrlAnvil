using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlConstraintBuilderCollection : SqlConstraintBuilderCollection
{
    internal PostgreSqlConstraintBuilderCollection() { }

    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    [Pure]
    public new PostgreSqlPrimaryKeyBuilder GetPrimaryKey()
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.GetPrimaryKey() );
    }

    [Pure]
    public new PostgreSqlPrimaryKeyBuilder? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.TryGetPrimaryKey() );
    }

    [Pure]
    public new PostgreSqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.GetIndex( name ) );
    }

    [Pure]
    public new PostgreSqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new PostgreSqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new PostgreSqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new PostgreSqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.GetCheck( name ) );
    }

    [Pure]
    public new PostgreSqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    public PostgreSqlPrimaryKeyBuilder SetPrimaryKey(PostgreSqlIndexBuilder index)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.SetPrimaryKey( index ) );
    }

    public PostgreSqlPrimaryKeyBuilder SetPrimaryKey(string name, PostgreSqlIndexBuilder index)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.SetPrimaryKey( name, index ) );
    }

    public new PostgreSqlIndexBuilder CreateIndex(ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.CreateIndex( columns, isUnique ) );
    }

    public new PostgreSqlIndexBuilder CreateIndex(string name, ReadOnlyArray<SqlOrderByNode> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<PostgreSqlIndexBuilder>( base.CreateIndex( name, columns, isUnique ) );
    }

    public PostgreSqlForeignKeyBuilder CreateForeignKey(PostgreSqlIndexBuilder originIndex, PostgreSqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    public PostgreSqlForeignKeyBuilder CreateForeignKey(string name, PostgreSqlIndexBuilder originIndex, PostgreSqlIndexBuilder referencedIndex)
    {
        return ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    public new PostgreSqlCheckBuilder CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.CreateCheck( condition ) );
    }

    public new PostgreSqlCheckBuilder CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<PostgreSqlCheckBuilder>( base.CreateCheck( name, condition ) );
    }
}
