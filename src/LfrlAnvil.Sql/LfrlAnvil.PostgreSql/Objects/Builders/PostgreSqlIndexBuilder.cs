using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlIndexBuilder : SqlIndexBuilder
{
    internal PostgreSqlIndexBuilder(
        PostgreSqlTableBuilder table,
        string name,
        SqlIndexBuilderColumns<PostgreSqlColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlIndexBuilder.PrimaryKey" />
    public new PostgreSqlPrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( base.PrimaryKey );

    /// <inheritdoc cref="SqlIndexBuilder.Columns" />
    public new SqlIndexBuilderColumns<PostgreSqlColumnBuilder> Columns =>
        new SqlIndexBuilderColumns<PostgreSqlColumnBuilder>( base.Columns.Expressions );

    /// <inheritdoc cref="SqlIndexBuilder.ReferencedColumns" />
    public new SqlObjectBuilderArray<PostgreSqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<PostgreSqlColumnBuilder>();

    /// <inheritdoc cref="SqlIndexBuilder.ReferencedFilterColumns" />
    public new SqlObjectBuilderArray<PostgreSqlColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<PostgreSqlColumnBuilder>();

    /// <inheritdoc cref="SqlIndexBuilder.SetName(string)" />
    public new PostgreSqlIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetDefaultName()" />
    public new PostgreSqlIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.MarkAsUnique(bool)" />
    public new PostgreSqlIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.MarkAsVirtual(bool)" />
    public new PostgreSqlIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetFilter(SqlConditionNode)" />
    public new PostgreSqlIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
