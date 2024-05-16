using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteCheckBuilder : SqlCheckBuilder
{
    internal SqliteCheckBuilder(
        SqliteTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, condition, referencedColumns ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlCheckBuilder.ReferencedColumns" />
    public new SqlObjectBuilderArray<SqliteColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<SqliteColumnBuilder>();

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteCheckBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlCheckBuilder.SetName(string)" />
    public new SqliteCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlCheckBuilder.SetDefaultName()" />
    public new SqliteCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
