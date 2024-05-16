using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlTableBuilder : SqlTableBuilder
{
    internal MySqlTableBuilder(MySqlSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new MySqlColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new MySqlConstraintBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlTableBuilder.Schema" />
    public new MySqlSchemaBuilder Schema => ReinterpretCast.To<MySqlSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlTableBuilder.Columns" />
    public new MySqlColumnBuilderCollection Columns => ReinterpretCast.To<MySqlColumnBuilderCollection>( base.Columns );

    /// <inheritdoc cref="SqlTableBuilder.Constraints" />
    public new MySqlConstraintBuilderCollection Constraints => ReinterpretCast.To<MySqlConstraintBuilderCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTableBuilder.SetName(string)" />
    public new MySqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
