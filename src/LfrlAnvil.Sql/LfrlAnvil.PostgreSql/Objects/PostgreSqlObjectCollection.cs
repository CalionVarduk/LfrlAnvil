using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlObjectCollection : SqlObjectCollection
{
    internal PostgreSqlObjectCollection(PostgreSqlObjectBuilderCollection source)
        : base( source ) { }

    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );

    [Pure]
    public new PostgreSqlTable GetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTable>( base.GetTable( name ) );
    }

    [Pure]
    public new PostgreSqlTable? TryGetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTable>( base.TryGetTable( name ) );
    }

    [Pure]
    public new PostgreSqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.GetIndex( name ) );
    }

    [Pure]
    public new PostgreSqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new PostgreSqlPrimaryKey GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKey>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new PostgreSqlPrimaryKey? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKey>( base.TryGetPrimaryKey( name ) );
    }

    [Pure]
    public new PostgreSqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new PostgreSqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new PostgreSqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.GetCheck( name ) );
    }

    [Pure]
    public new PostgreSqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.TryGetCheck( name ) );
    }

    [Pure]
    public new PostgreSqlView GetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlView>( base.GetView( name ) );
    }

    [Pure]
    public new PostgreSqlView? TryGetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlView>( base.TryGetView( name ) );
    }

    [Pure]
    protected override PostgreSqlTable CreateTable(SqlTableBuilder builder)
    {
        return new PostgreSqlTable( Schema, ReinterpretCast.To<PostgreSqlTableBuilder>( builder ) );
    }

    [Pure]
    protected override PostgreSqlView CreateView(SqlViewBuilder builder)
    {
        return new PostgreSqlView( Schema, ReinterpretCast.To<PostgreSqlViewBuilder>( builder ) );
    }

    [Pure]
    protected override PostgreSqlIndex CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new PostgreSqlIndex( ReinterpretCast.To<PostgreSqlTable>( table ), ReinterpretCast.To<PostgreSqlIndexBuilder>( builder ) );
    }

    [Pure]
    protected override PostgreSqlPrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new PostgreSqlPrimaryKey( ReinterpretCast.To<PostgreSqlIndex>( index ), ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( builder ) );
    }

    [Pure]
    protected override PostgreSqlCheck CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new PostgreSqlCheck( ReinterpretCast.To<PostgreSqlTable>( table ), ReinterpretCast.To<PostgreSqlCheckBuilder>( builder ) );
    }

    [Pure]
    protected override PostgreSqlForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new PostgreSqlForeignKey(
            ReinterpretCast.To<PostgreSqlIndex>( originIndex ),
            ReinterpretCast.To<PostgreSqlIndex>( referencedIndex ),
            ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( builder ) );
    }
}
