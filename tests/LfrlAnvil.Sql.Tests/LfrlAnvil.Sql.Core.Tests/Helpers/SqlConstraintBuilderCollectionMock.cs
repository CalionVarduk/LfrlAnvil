using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlConstraintBuilderCollectionMock : SqlConstraintBuilderCollection
{
    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );

    [Pure]
    public new SqlPrimaryKeyBuilderMock GetPrimaryKey()
    {
        return ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.GetPrimaryKey() );
    }

    [Pure]
    public new SqlPrimaryKeyBuilderMock? TryGetPrimaryKey()
    {
        return ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.TryGetPrimaryKey() );
    }

    [Pure]
    public new SqlIndexBuilderMock GetIndex(string name)
    {
        return ReinterpretCast.To<SqlIndexBuilderMock>( base.GetIndex( name ) );
    }

    [Pure]
    public new SqlIndexBuilderMock? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqlIndexBuilderMock>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new SqlForeignKeyBuilderMock GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqlForeignKeyBuilderMock>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new SqlForeignKeyBuilderMock? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqlForeignKeyBuilderMock>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new SqlCheckBuilderMock GetCheck(string name)
    {
        return ReinterpretCast.To<SqlCheckBuilderMock>( base.GetCheck( name ) );
    }

    [Pure]
    public new SqlCheckBuilderMock? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqlCheckBuilderMock>( base.TryGetCheck( name ) );
    }

    public SqlPrimaryKeyBuilderMock SetPrimaryKey(SqlIndexBuilderMock index)
    {
        return ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.SetPrimaryKey( index ) );
    }

    public SqlPrimaryKeyBuilderMock SetPrimaryKey(string name, SqlIndexBuilderMock index)
    {
        return ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.SetPrimaryKey( name, index ) );
    }

    public new SqlIndexBuilderMock CreateIndex(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns, bool isUnique = false)
    {
        return ReinterpretCast.To<SqlIndexBuilderMock>( base.CreateIndex( columns, isUnique ) );
    }

    public new SqlIndexBuilderMock CreateIndex(
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique = false)
    {
        return ReinterpretCast.To<SqlIndexBuilderMock>( base.CreateIndex( name, columns, isUnique ) );
    }

    public SqlForeignKeyBuilderMock CreateForeignKey(SqlIndexBuilderMock originIndex, SqlIndexBuilderMock referencedIndex)
    {
        return ReinterpretCast.To<SqlForeignKeyBuilderMock>( base.CreateForeignKey( originIndex, referencedIndex ) );
    }

    public SqlForeignKeyBuilderMock CreateForeignKey(string name, SqlIndexBuilderMock originIndex, SqlIndexBuilderMock referencedIndex)
    {
        return ReinterpretCast.To<SqlForeignKeyBuilderMock>( base.CreateForeignKey( name, originIndex, referencedIndex ) );
    }

    public new SqlCheckBuilderMock CreateCheck(SqlConditionNode condition)
    {
        return ReinterpretCast.To<SqlCheckBuilderMock>( base.CreateCheck( condition ) );
    }

    public new SqlCheckBuilderMock CreateCheck(string name, SqlConditionNode condition)
    {
        return ReinterpretCast.To<SqlCheckBuilderMock>( base.CreateCheck( name, condition ) );
    }
}
