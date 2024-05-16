using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteIndexBuilder : SqlIndexBuilder
{
    internal SqliteIndexBuilder(
        SqliteTableBuilder table,
        string name,
        SqlIndexBuilderColumns<SqliteColumnBuilder> columns,
        bool isUnique,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, new SqlIndexBuilderColumns<SqlColumnBuilder>( columns.Expressions ), isUnique, referencedColumns ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlIndexBuilder.PrimaryKey" />
    public new SqlitePrimaryKeyBuilder? PrimaryKey => ReinterpretCast.To<SqlitePrimaryKeyBuilder>( base.PrimaryKey );

    /// <inheritdoc cref="SqlIndexBuilder.Columns" />
    public new SqlIndexBuilderColumns<SqliteColumnBuilder> Columns =>
        new SqlIndexBuilderColumns<SqliteColumnBuilder>( base.Columns.Expressions );

    /// <inheritdoc cref="SqlIndexBuilder.ReferencedColumns" />
    public new SqlObjectBuilderArray<SqliteColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<SqliteColumnBuilder>();

    /// <inheritdoc cref="SqlIndexBuilder.ReferencedFilterColumns" />
    public new SqlObjectBuilderArray<SqliteColumnBuilder> ReferencedFilterColumns =>
        base.ReferencedFilterColumns.UnsafeReinterpretAs<SqliteColumnBuilder>();

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteIndexBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetName(string)" />
    public new SqliteIndexBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetDefaultName()" />
    public new SqliteIndexBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.MarkAsUnique(bool)" />
    public new SqliteIndexBuilder MarkAsUnique(bool enabled = true)
    {
        base.MarkAsUnique( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.MarkAsVirtual(bool)" />
    public new SqliteIndexBuilder MarkAsVirtual(bool enabled = true)
    {
        base.MarkAsVirtual( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlIndexBuilder.SetFilter(SqlConditionNode)" />
    public new SqliteIndexBuilder SetFilter(SqlConditionNode? filter)
    {
        base.SetFilter( filter );
        return this;
    }
}
