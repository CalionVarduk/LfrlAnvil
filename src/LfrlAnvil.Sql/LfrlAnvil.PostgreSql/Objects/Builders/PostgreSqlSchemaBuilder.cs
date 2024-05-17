using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlSchemaBuilder : SqlSchemaBuilder
{
    internal PostgreSqlSchemaBuilder(PostgreSqlDatabaseBuilder database, string name)
        : base( database, name, new PostgreSqlObjectBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilder.Objects" />
    public new PostgreSqlObjectBuilderCollection Objects => ReinterpretCast.To<PostgreSqlObjectBuilderCollection>( base.Objects );

    /// <inheritdoc cref="SqlSchemaBuilder.SetName(string)" />
    public new PostgreSqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
