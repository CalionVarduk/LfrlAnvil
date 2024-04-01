using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlObjectBuilderCollection : SqlObjectBuilderCollection
{
    internal PostgreSqlObjectBuilderCollection() { }

    public new PostgreSqlSchemaBuilder Schema => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Schema );

    [Pure]
    public new PostgreSqlTableBuilder GetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.GetTable( name ) );
    }

    [Pure]
    public new PostgreSqlTableBuilder? TryGetTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.TryGetTable( name ) );
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
    public new PostgreSqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new PostgreSqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.TryGetPrimaryKey( name ) );
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

    [Pure]
    public new PostgreSqlViewBuilder GetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewBuilder>( base.GetView( name ) );
    }

    [Pure]
    public new PostgreSqlViewBuilder? TryGetView(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewBuilder>( base.TryGetView( name ) );
    }

    public new PostgreSqlTableBuilder CreateTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.CreateTable( name ) );
    }

    public new PostgreSqlTableBuilder GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<PostgreSqlTableBuilder>( base.GetOrCreateTable( name ) );
    }

    public new PostgreSqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<PostgreSqlViewBuilder>( base.CreateView( name, source ) );
    }

    protected override PostgreSqlTableBuilder CreateTableBuilder(string name)
    {
        return new PostgreSqlTableBuilder( Schema, name );
    }

    protected override PostgreSqlViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new PostgreSqlViewBuilder( Schema, name, source, referencedObjects );
    }

    protected override PostgreSqlIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new PostgreSqlIndexBuilder(
            ReinterpretCast.To<PostgreSqlTableBuilder>( table ),
            name,
            new SqlIndexBuilderColumns<PostgreSqlColumnBuilder>( columns.Expressions ),
            isUnique,
            referencedColumns );
    }

    protected override PostgreSqlPrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new PostgreSqlPrimaryKeyBuilder( ReinterpretCast.To<PostgreSqlIndexBuilder>( index ), name );
    }

    protected override PostgreSqlForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new PostgreSqlForeignKeyBuilder(
            ReinterpretCast.To<PostgreSqlIndexBuilder>( originIndex ),
            ReinterpretCast.To<PostgreSqlIndexBuilder>( referencedIndex ),
            name );
    }

    protected override PostgreSqlCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new PostgreSqlCheckBuilder( ReinterpretCast.To<PostgreSqlTableBuilder>( table ), name, condition, referencedColumns );
    }
}
