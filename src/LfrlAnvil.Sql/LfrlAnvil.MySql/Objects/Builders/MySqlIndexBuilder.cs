using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlIndexBuilder : SqlIndexBuilder
{
    internal MySqlIndexBuilder(
        MySqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<MySqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlIndexBuilder.PrimaryKey" />
    public new MySqlPrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<MySqlPrimaryKeyBuilder>( base.PrimaryKey );

    /// <inheritdoc cref="SqlIndexBuilder.Columns" />
    public new SqlIndexBuilderColumns<MySqlColumnBuilder> Columns =>
        new SqlIndexBuilderColumns<MySqlColumnBuilder>( base.Columns.Expressions );

    /// <inheritdoc cref="SqlIndexBuilder.ReferencedColumns" />
    public new SqlObjectBuilderArray<MySqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<MySqlColumnBuilder>();

    /// <inheritdoc cref="SqlIndexBuilder.ReferencedFilterColumns" />
    public new SqlObjectBuilderArray<MySqlColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<MySqlColumnBuilder>();

    /// <inheritdoc cref="SqlIndexBuilder.SetName(string)" />
    public new MySqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetDefaultName()" />
    public new MySqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.MarkAsUnique(bool)" />
    public new MySqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.MarkAsVirtual(bool)" />
    public new MySqlIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetFilter(SqlConditionNode)" />
    public new MySqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }

    /// <inheritdoc />
    protected override SqlPropertyChange<SqlConditionNode?> BeforeFilterChange(SqlConditionNode? newValue)
    {
        if ( ReferenceEquals( Filter, newValue ) || Database.IndexFilterResolution == SqlOptionalFunctionalityResolution.Ignore )
            return SqlPropertyChange.Cancel<SqlConditionNode?>();

        if ( newValue is not null && Database.IndexFilterResolution == SqlOptionalFunctionalityResolution.Forbid )
            throw SqlHelpers.CreateObjectBuilderException( Database, Resources.IndexFiltersAreForbidden( this, newValue ) );

        return base.BeforeFilterChange( newValue );
    }
}
