using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlObjectCollection : SqlObjectCollection
{
    internal PostgreSqlObjectCollection(PostgreSqlObjectBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlObjectCollection.Schema" />
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObjectCollection.GetTable(string)" />
    [Pure]
    public new PostgreSqlTable GetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTable>( base.GetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetTable(string)" />
    [Pure]
    public new PostgreSqlTable? TryGetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTable>( base.TryGetTable( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetIndex(string)" />
    [Pure]
    public new PostgreSqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetIndex(string)" />
    [Pure]
    public new PostgreSqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetPrimaryKey(string)" />
    [Pure]
    public new PostgreSqlPrimaryKey GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKey>( base.GetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetPrimaryKey(string)" />
    [Pure]
    public new PostgreSqlPrimaryKey? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKey>( base.TryGetPrimaryKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetForeignKey(string)" />
    [Pure]
    public new PostgreSqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetCheck(string)" />
    [Pure]
    public new PostgreSqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetCheck(string)" />
    [Pure]
    public new PostgreSqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.TryGetCheck( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.GetView(string)" />
    [Pure]
    public new PostgreSqlView GetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlView>( base.GetView( name ) );
    }

    /// <inheritdoc cref="SqlObjectCollection.TryGetView(string)" />
    [Pure]
    public new PostgreSqlView? TryGetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlView>( base.TryGetView( name ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override PostgreSqlTable CreateTable(SqlTableBuilder builder)
    {
        return new PostgreSqlTable( Schema, ReinterpretCast.To<PostgreSqlTableBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override PostgreSqlView CreateView(SqlViewBuilder builder)
    {
        return new PostgreSqlView( Schema, ReinterpretCast.To<PostgreSqlViewBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override PostgreSqlIndex CreateIndex(SqlTable table, SqlIndexBuilder builder)
    {
        return new PostgreSqlIndex( ReinterpretCast.To<PostgreSqlTable>( table ), ReinterpretCast.To<PostgreSqlIndexBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override PostgreSqlPrimaryKey CreatePrimaryKey(SqlIndex index, SqlPrimaryKeyBuilder builder)
    {
        return new PostgreSqlPrimaryKey(
            ReinterpretCast.To<PostgreSqlIndex>( index ),
            ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override PostgreSqlCheck CreateCheck(SqlTable table, SqlCheckBuilder builder)
    {
        return new PostgreSqlCheck( ReinterpretCast.To<PostgreSqlTable>( table ), ReinterpretCast.To<PostgreSqlCheckBuilder>( builder ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override PostgreSqlForeignKey CreateForeignKey(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
    {
        return new PostgreSqlForeignKey(
            ReinterpretCast.To<PostgreSqlIndex>( originIndex ),
            ReinterpretCast.To<PostgreSqlIndex>( referencedIndex ),
            ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( builder ) );
    }
}
