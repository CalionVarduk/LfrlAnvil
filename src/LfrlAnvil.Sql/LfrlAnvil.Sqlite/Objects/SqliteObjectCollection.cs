using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteObjectCollection : SqlObjectCollection
{
    internal SqliteObjectCollection(SqliteObjectBuilderCollection source)
        : base( source ) { }

    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );

    [Pure]
    public new SqliteTable GetTable(string name)
    {
        return ReinterpretCast.To<SqliteTable>( base.GetTable( name ) );
    }

    [Pure]
    public new SqliteTable? TryGetTable(string name)
    {
        return ReinterpretCast.To<SqliteTable>( base.TryGetTable( name ) );
    }

    [Pure]
    public new SqliteIndex GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.GetIndex( name ) );
    }

    [Pure]
    public new SqliteIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new SqlitePrimaryKey GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKey>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new SqlitePrimaryKey? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKey>( base.TryGetPrimaryKey( name ) );
    }

    [Pure]
    public new SqliteForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new SqliteForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new SqliteCheck GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.GetCheck( name ) );
    }

    [Pure]
    public new SqliteCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.TryGetCheck( name ) );
    }

    [Pure]
    public new SqliteView GetView(string name)
    {
        return ReinterpretCast.To<SqliteView>( base.GetView( name ) );
    }

    [Pure]
    public new SqliteView? TryGetView(string name)
    {
        return ReinterpretCast.To<SqliteView>( base.TryGetView( name ) );
    }

    [Pure]
    protected override SqliteTable CreateTable(SqlTableBuilder builder)
    {
        return new SqliteTable( Schema, ReinterpretCast.To<SqliteTableBuilder>( builder ) );
    }

    [Pure]
    protected override SqliteView CreateView(SqlViewBuilder builder)
    {
        return new SqliteView( Schema, ReinterpretCast.To<SqliteViewBuilder>( builder ) );
    }

    [Pure]
    protected override SqliteIndex CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new SqliteIndex( ReinterpretCast.To<SqliteTable>( table ), ReinterpretCast.To<SqliteIndexBuilder>( builder ) );
    }

    [Pure]
    protected override SqlitePrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new SqlitePrimaryKey( ReinterpretCast.To<SqliteIndex>( index ), ReinterpretCast.To<SqlitePrimaryKeyBuilder>( builder ) );
    }

    [Pure]
    protected override SqliteCheck CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new SqliteCheck( ReinterpretCast.To<SqliteTable>( table ), ReinterpretCast.To<SqliteCheckBuilder>( builder ) );
    }

    [Pure]
    protected override SqliteForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new SqliteForeignKey(
            ReinterpretCast.To<SqliteIndex>( originIndex ),
            ReinterpretCast.To<SqliteIndex>( referencedIndex ),
            ReinterpretCast.To<SqliteForeignKeyBuilder>( builder ) );
    }
}
