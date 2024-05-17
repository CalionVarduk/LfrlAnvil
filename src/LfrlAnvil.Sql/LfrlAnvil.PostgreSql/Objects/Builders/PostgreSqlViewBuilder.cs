using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlViewBuilder : SqlViewBuilder
{
    internal PostgreSqlViewBuilder(
        PostgreSqlSchemaBuilder schema,
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
        : base( schema, name, source, referencedObjects ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlViewBuilder.Schema" />
    public new PostgreSqlSchemaBuilder Schema => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlViewBuilder.SetName(string)" />
    public new PostgreSqlViewBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
