using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlObjectBuilderCollection : SqlObjectBuilderCollection
{
    internal MySqlObjectBuilderCollection() { }

    public new MySqlSchemaBuilder Schema => ReinterpretCast.To<MySqlSchemaBuilder>( base.Schema );

    [Pure]
    public new MySqlTableBuilder GetTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.GetTable( name ) );
    }

    [Pure]
    public new MySqlTableBuilder? TryGetTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.TryGetTable( name ) );
    }

    [Pure]
    public new MySqlIndexBuilder GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.GetIndex( name ) );
    }

    [Pure]
    public new MySqlIndexBuilder? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndexBuilder>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new MySqlPrimaryKeyBuilder GetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.GetPrimaryKey( name ) );
    }

    [Pure]
    public new MySqlPrimaryKeyBuilder? TryGetPrimaryKey(string name)
    {
        return ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.TryGetPrimaryKey( name ) );
    }

    [Pure]
    public new MySqlForeignKeyBuilder GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new MySqlForeignKeyBuilder? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKeyBuilder>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new MySqlCheckBuilder GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.GetCheck( name ) );
    }

    [Pure]
    public new MySqlCheckBuilder? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheckBuilder>( base.TryGetCheck( name ) );
    }

    [Pure]
    public new MySqlViewBuilder GetView(string name)
    {
        return ReinterpretCast.To<MySqlViewBuilder>( base.GetView( name ) );
    }

    [Pure]
    public new MySqlViewBuilder? TryGetView(string name)
    {
        return ReinterpretCast.To<MySqlViewBuilder>( base.TryGetView( name ) );
    }

    public new MySqlTableBuilder CreateTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.CreateTable( name ) );
    }

    public new MySqlTableBuilder GetOrCreateTable(string name)
    {
        return ReinterpretCast.To<MySqlTableBuilder>( base.GetOrCreateTable( name ) );
    }

    public new MySqlViewBuilder CreateView(string name, SqlQueryExpressionNode source)
    {
        return ReinterpretCast.To<MySqlViewBuilder>( base.CreateView( name, source ) );
    }

    protected override MySqlTableBuilder CreateTableBuilder(string name)
    {
        return new MySqlTableBuilder( Schema, name );
    }

    protected override MySqlViewBuilder CreateViewBuilder(
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
    {
        return new MySqlViewBuilder( Schema, name, source, referencedObjects );
    }

    protected override MySqlIndexBuilder CreateIndexBuilder(
        SqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new MySqlIndexBuilder(
            ReinterpretCast.To<MySqlTableBuilder>( table ),
            name,
            new SqlIndexBuilderColumns<MySqlColumnBuilder>( columns.Expressions ),
            isUnique,
            referencedColumns );
    }

    protected override MySqlPrimaryKeyBuilder CreatePrimaryKeyBuilder(string name, SqlIndexBuilder index)
    {
        return new MySqlPrimaryKeyBuilder( ReinterpretCast.To<MySqlIndexBuilder>( index ), name );
    }

    protected override MySqlForeignKeyBuilder CreateForeignKeyBuilder(
        string name,
        SqlIndexBuilder originIndex,
        SqlIndexBuilder referencedIndex)
    {
        return new MySqlForeignKeyBuilder(
            ReinterpretCast.To<MySqlIndexBuilder>( originIndex ),
            ReinterpretCast.To<MySqlIndexBuilder>( referencedIndex ),
            name );
    }

    protected override MySqlCheckBuilder CreateCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
    {
        return new MySqlCheckBuilder( ReinterpretCast.To<MySqlTableBuilder>( table ), name, condition, referencedColumns );
    }
}
