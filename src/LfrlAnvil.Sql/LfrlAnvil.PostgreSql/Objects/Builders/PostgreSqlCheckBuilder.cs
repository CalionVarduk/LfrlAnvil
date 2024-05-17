using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlCheckBuilder : SqlCheckBuilder
{
    internal PostgreSqlCheckBuilder(
        PostgreSqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, name, condition, referencedColumns ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlCheckBuilder.ReferencedColumns" />
    public new SqlObjectBuilderArray<PostgreSqlColumnBuilder> ReferencedColumns =>
        base.ReferencedColumns.UnsafeReinterpretAs<PostgreSqlColumnBuilder>();

    /// <inheritdoc cref="SqlCheckBuilder.SetName(string)" />
    public new PostgreSqlCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlCheckBuilder.SetDefaultName()" />
    public new PostgreSqlCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
