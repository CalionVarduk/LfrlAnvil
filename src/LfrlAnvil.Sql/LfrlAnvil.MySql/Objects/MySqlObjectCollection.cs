using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlObjectCollection : SqlObjectCollection
{
    internal MySqlObjectCollection(MySqlObjectBuilderCollection source)
        : base( source ) { }

    public new MySqlSchema Schema => ReinterpretCast.To<MySqlSchema>( base.Schema );

    [Pure]
    public new MySqlTable GetTable(string name)
    {
        return ReinterpretCast.To<MySqlTable>( base.GetTable( name ) );
    }

    [Pure]
    public new MySqlTable? TryGetTable(string name)
    {
        return ReinterpretCast.To<MySqlTable>( base.TryGetTable( name ) );
    }

    [Pure]
    public new MySqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.GetIndex( name ) );
    }

    [Pure]
    public new MySqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new MySqlPrimaryKey GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKey>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new MySqlPrimaryKey? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKey>( base.TryGetPrimaryKey( name ) );
    }

    [Pure]
    public new MySqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new MySqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new MySqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.GetCheck( name ) );
    }

    [Pure]
    public new MySqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.TryGetCheck( name ) );
    }

    [Pure]
    public new MySqlView GetView(string name)
    {
        return ReinterpretCast.To<MySqlView>( base.GetView( name ) );
    }

    [Pure]
    public new MySqlView? TryGetView(string name)
    {
        return ReinterpretCast.To<MySqlView>( base.TryGetView( name ) );
    }

    [Pure]
    protected override MySqlTable CreateTable(SqlTableBuilder builder)
    {
        return new MySqlTable( Schema, ReinterpretCast.To<MySqlTableBuilder>( builder ) );
    }

    [Pure]
    protected override MySqlView CreateView(SqlViewBuilder builder)
    {
        return new MySqlView( Schema, ReinterpretCast.To<MySqlViewBuilder>( builder ) );
    }

    [Pure]
    protected override MySqlIndex CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new MySqlIndex( ReinterpretCast.To<MySqlTable>( table ), ReinterpretCast.To<MySqlIndexBuilder>( builder ) );
    }

    [Pure]
    protected override MySqlPrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new MySqlPrimaryKey( ReinterpretCast.To<MySqlIndex>( index ), ReinterpretCast.To<MySqlPrimaryKeyBuilder>( builder ) );
    }

    [Pure]
    protected override MySqlCheck CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new MySqlCheck( ReinterpretCast.To<MySqlTable>( table ), ReinterpretCast.To<MySqlCheckBuilder>( builder ) );
    }

    [Pure]
    protected override MySqlForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new MySqlForeignKey(
            ReinterpretCast.To<MySqlIndex>( originIndex ),
            ReinterpretCast.To<MySqlIndex>( referencedIndex ),
            ReinterpretCast.To<MySqlForeignKeyBuilder>( builder ) );
    }
}
