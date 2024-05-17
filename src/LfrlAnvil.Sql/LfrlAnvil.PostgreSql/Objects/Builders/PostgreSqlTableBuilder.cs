using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlTableBuilder : SqlTableBuilder
{
    internal PostgreSqlTableBuilder(PostgreSqlSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new PostgreSqlColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new PostgreSqlConstraintBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlTableBuilder.Schema" />
    public new PostgreSqlSchemaBuilder Schema => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlTableBuilder.Columns" />
    public new PostgreSqlColumnBuilderCollection Columns => ReinterpretCast.To<PostgreSqlColumnBuilderCollection>( base.Columns );

    /// <inheritdoc cref="SqlTableBuilder.Constraints" />
    public new PostgreSqlConstraintBuilderCollection Constraints =>
        ReinterpretCast.To<PostgreSqlConstraintBuilderCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTableBuilder.SetName(string)" />
    public new PostgreSqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
