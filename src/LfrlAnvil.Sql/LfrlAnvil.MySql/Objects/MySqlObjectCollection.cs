using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlObjectCollection : SqlObjectCollection
{
    internal MySqlObjectCollection(MySqlObjectBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlObjectCollection.Schema" />
    public new MySqlSchema Schema => ReinterpretCast.To<MySqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObjectCollection.GetTable(string)" />
    [Pure]
    public new MySqlTable GetTable(string name)
    {
        return ReinterpretCast.To<MySqlTable>( base.GetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetTable(string)" />
    [Pure]
    public new MySqlTable? TryGetTable(string name)
    {
        return ReinterpretCast.To<MySqlTable>( base.TryGetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetIndex(string)" />
    [Pure]
    public new MySqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetIndex(string)" />
    [Pure]
    public new MySqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetPrimaryKey(string)" />
    [Pure]
    public new MySqlPrimaryKey GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKey>( base.GetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public new MySqlPrimaryKey? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKey>( base.TryGetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetCheck(string)" />
    [Pure]
    public new MySqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetCheck(string)" />
    [Pure]
    public new MySqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetView(string)" />
    [Pure]
    public new MySqlView GetView(string name)
    {
        return ReinterpretCast.To<MySqlView>( base.GetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetView(string)" />
    [Pure]
    public new MySqlView? TryGetView(string name)
    {
        return ReinterpretCast.To<MySqlView>( base.TryGetView( name ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlTable CreateTable(SqlTableBuilder builder)
    {
        return new MySqlTable( Schema, ReinterpretCast.To<MySqlTableBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlView CreateView(SqlViewBuilder builder)
    {
        return new MySqlView( Schema, ReinterpretCast.To<MySqlViewBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlIndex CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new MySqlIndex( ReinterpretCast.To<MySqlTable>( table ), ReinterpretCast.To<MySqlIndexBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlPrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new MySqlPrimaryKey( ReinterpretCast.To<MySqlIndex>( index ), ReinterpretCast.To<MySqlPrimaryKeyBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlCheck CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new MySqlCheck( ReinterpretCast.To<MySqlTable>( table ), ReinterpretCast.To<MySqlCheckBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new MySqlForeignKey(
            ReinterpretCast.To<MySqlIndex>( originIndex ),
            ReinterpretCast.To<MySqlIndex>( referencedIndex ),
            ReinterpretCast.To<MySqlForeignKeyBuilder>( builder ) );
    }
}
