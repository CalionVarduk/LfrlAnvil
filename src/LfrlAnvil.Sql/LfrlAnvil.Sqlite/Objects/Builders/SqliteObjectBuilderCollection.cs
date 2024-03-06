using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteObjectBuilderCollection : SqlObjectBuilderCollection
{
    internal SqliteObjectBuilderCollection() { }

    public new SqliteSchemaBuilder Schema => ReinterpretCast.To<SqliteSchemaBuilder>( base.Schema );

    [Pure]
    public new SqliteTableBuilder GetTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.GetTable( name ) );
    }

    [Pure]
    public new SqliteTableBuilder? TryGetTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.TryGetTable( name ) );
    }

    [Pure]
    public new SqliteIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.GetIndex( name ) );
    }

    [Pure]
    public new SqliteIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndexBuilder>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new SqlitePrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new SqlitePrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.TryGetPrimaryKey( name ) );
    }

    [Pure]
    public new SqliteForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new SqliteForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new SqliteCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.GetCheck( name ) );
    }

    [Pure]
    public new SqliteCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheckBuilder>( base.TryGetCheck( name ) );
    }

    [Pure]
    public new SqliteViewBuilder GetView(string name)
    {
        return ReinterpretCast.To<SqliteViewBuilder>( base.GetView( name ) );
    }

    [Pure]
    public new SqliteViewBuilder? TryGetView(string name)
    {
        return ReinterpretCast.To<SqliteViewBuilder>( base.TryGetView( name ) );
    }

    public new SqliteTableBuilder CreateTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.CreateTable( name ) );
    }

    public new SqliteTableBuilder GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<SqliteTableBuilder>( base.GetOrCreateTable( name ) );
    }

    public new SqliteViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<SqliteViewBuilder>( base.CreateView( name, source ) );
    }

    protected override SqliteTableBuilder CreateTableBuilder(string name)
    {
        return new SqliteTableBuilder( Schema, name );
    }

    protected override SqliteViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new SqliteViewBuilder( Schema, name, source, referencedObjects );
    }

    protected override SqliteIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns,
        bool isUnique)
    {
        return new SqliteIndexBuilder( ReinterpretCast.To<SqliteTableBuilder>( table ), name, columns, isUnique );
    }

    protected override SqlitePrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new SqlitePrimaryKeyBuilder( ReinterpretCast.To<SqliteIndexBuilder>( index ), name );
    }

    protected override SqliteForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new SqliteForeignKeyBuilder(
            ReinterpretCast.To<SqliteIndexBuilder>( originIndex ),
            ReinterpretCast.To<SqliteIndexBuilder>( referencedIndex ),
            name );
    }

    protected override SqliteCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new SqliteCheckBuilder( ReinterpretCast.To<SqliteTableBuilder>( table ), name, condition, referencedColumns );
    }
}
