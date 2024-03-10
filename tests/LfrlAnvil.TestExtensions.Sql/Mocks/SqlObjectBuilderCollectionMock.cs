using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlObjectBuilderCollectionMock : SqlObjectBuilderCollection
{
    public new SqlSchemaBuilderMock Schema => ReinterpretCast.To<SqlSchemaBuilderMock>( base.Schema );

    [Pure]
    public new SqlTableBuilderMock GetTable(string name)
    {
        return ReinterpretCast.To<SqlTableBuilderMock>( base.GetTable( name ) );
    }

    [Pure]
    public new SqlTableBuilderMock? TryGetTable(string name)
    {
        return ReinterpretCast.To<SqlTableBuilderMock>( base.TryGetTable( name ) );
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
    public new SqlPrimaryKeyBuilderMock GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new SqlPrimaryKeyBuilderMock? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlPrimaryKeyBuilderMock>( base.TryGetPrimaryKey( name ) );
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

    [Pure]
    public new SqlViewBuilderMock GetView(string name)
    {
        return ReinterpretCast.To<SqlViewBuilderMock>( base.GetView( name ) );
    }

    [Pure]
    public new SqlViewBuilderMock? TryGetView(string name)
    {
        return ReinterpretCast.To<SqlViewBuilderMock>( base.TryGetView( name ) );
    }

    public new SqlTableBuilderMock CreateTable(string name)
    {
        return ReinterpretCast.To<SqlTableBuilderMock>( base.CreateTable( name ) );
    }

    public new SqlTableBuilderMock GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<SqlTableBuilderMock>( base.GetOrCreateTable( name ) );
    }

    public new SqlViewBuilderMock CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<SqlViewBuilderMock>( base.CreateView( name, source ) );
    }

    protected override SqlTableBuilderMock CreateTableBuilder(string name)
    {
        return new SqlTableBuilderMock( Schema, name );
    }

    protected override SqlViewBuilderMock CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new SqlViewBuilderMock( Schema, name, source, referencedObjects );
    }

    protected override SqlIndexBuilderMock CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new SqlIndexBuilderMock(
            ReinterpretCast.To<SqlTableBuilderMock>( table ),
            name,
            new SqlIndexBuilderColumns<SqlColumnBuilderMock>( columns.Expressions ),
            isUnique,
            referencedColumns );
    }

    protected override SqlPrimaryKeyBuilderMock CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new SqlPrimaryKeyBuilderMock( ReinterpretCast.To<SqlIndexBuilderMock>( index ), name );
    }

    protected override SqlForeignKeyBuilderMock CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new SqlForeignKeyBuilderMock(
            ReinterpretCast.To<SqlIndexBuilderMock>( originIndex ),
            ReinterpretCast.To<SqlIndexBuilderMock>( referencedIndex ),
            name );
    }

    protected override SqlCheckBuilderMock CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new SqlCheckBuilderMock( ReinterpretCast.To<SqlTableBuilderMock>( table ), name, condition, referencedColumns );
    }
}
