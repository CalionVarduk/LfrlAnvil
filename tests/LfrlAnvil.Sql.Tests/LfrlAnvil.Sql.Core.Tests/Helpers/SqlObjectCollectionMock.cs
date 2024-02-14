using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlObjectCollectionMock : SqlObjectCollection
{
    public SqlObjectCollectionMock(SqlObjectBuilderCollection source)
        : base( source ) { }

    [Pure]
    protected override SqlTableMock CreateTable(SqlTableBuilder builder)
    {
        return new SqlTableMock( Schema, builder );
    }

    [Pure]
    protected override SqlViewMock CreateView(SqlViewBuilder builder)
    {
        return new SqlViewMock( Schema, builder );
    }

    [Pure]
    protected override SqlIndexMock CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new SqlIndexMock( table, builder );
    }

    [Pure]
    protected override SqlPrimaryKeyMock CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new SqlPrimaryKeyMock( index, builder );
    }

    [Pure]
    protected override SqlCheckMock CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new SqlCheckMock( table, builder );
    }

    [Pure]
    protected override SqlForeignKeyMock CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new SqlForeignKeyMock( originIndex, referencedIndex, builder );
    }

    [Pure]
    protected override bool DeferCreation(SqlObjectBuilder builder)
    {
        if ( builder is not SqlUnknownObjectBuilderMock u || TryGetTable( u.Table.Name ) is null )
            return base.DeferCreation( builder );

        return u.DeferCreation && u.UseDefaultImplementation ? base.DeferCreation( builder ) : u.DeferCreation;
    }

    [Pure]
    protected override SqlObject CreateUnknown(SqlObjectBuilder builder)
    {
        if ( builder is not SqlUnknownObjectBuilderMock u || u.UseDefaultImplementation )
            return base.CreateUnknown( builder );

        var table = GetTable( u.Table.Name );
        return new SqlUnknownObjectMock( ReinterpretCast.To<SqlTableMock>( table ), u );
    }
}
