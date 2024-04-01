using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlIndexBuilder : SqlIndexBuilder
{
    internal PostgreSqlIndexBuilder(
        PostgreSqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<PostgreSqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );
    public new PostgreSqlPrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.PrimaryKey );

    public new SqlIndexBuilderColumns<PostgreSqlColumnBuilder> Columns =>
        new SqlIndexBuilderColumns<PostgreSqlColumnBuilder>( base.Columns.Expressions );

    public new SqlObjectBuilderArray<PostgreSqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<PostgreSqlColumnBuilder>();

    public new SqlObjectBuilderArray<PostgreSqlColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<PostgreSqlColumnBuilder>();

    public new PostgreSqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new PostgreSqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new PostgreSqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    public new PostgreSqlIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    public new PostgreSqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
