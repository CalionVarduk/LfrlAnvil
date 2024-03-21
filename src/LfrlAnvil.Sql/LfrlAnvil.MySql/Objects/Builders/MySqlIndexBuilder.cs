using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlIndexBuilder : SqlIndexBuilder
{
    internal MySqlIndexBuilder(
        MySqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<MySqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );
    public new MySqlPrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.PrimaryKey );

    public new SqlIndexBuilderColumns<MySqlColumnBuilder> Columns =>
        new SqlIndexBuilderColumns<MySqlColumnBuilder>( base.Columns.Expressions );

    public new SqlObjectBuilderArray<MySqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<MySqlColumnBuilder>();

    public new SqlObjectBuilderArray<MySqlColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<MySqlColumnBuilder>();

    public new MySqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new MySqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    public new MySqlIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    public new MySqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }

    protected override SqlPropertyChange<SqlConditionNode?> BeforeFilterChange(SqlConditionNode? newValue)
    {
        if ( ReferenceEquals( Filter, newValue ) || Database.IndexFilterResolution == SqlOptionalFunctionalityResolution.Ignore )
            return SqlPropertyChange.Cancel<SqlConditionNode?>();

        if ( newValue is not null && Database.IndexFilterResolution == SqlOptionalFunctionalityResolution.Forbid )
            throw SqlHelpers.CreateObjectBuilderException( Database, Resources.IndexFiltersAreForbidden( this, newValue ) );

        return base.BeforeFilterChange( newValue );
    }
}
