using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlTableBuilder : SqlTableBuilder
{
    internal PostgreSqlTableBuilder(PostgreSqlSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new PostgreSqlColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new PostgreSqlConstraintBuilderCollection() ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlSchemaBuilder Schema => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Schema );
    public new PostgreSqlColumnBuilderCollection Columns => ReinterpretCast.To<PostgreSqlColumnBuilderCollection>( base.Columns );
    public new PostgreSqlConstraintBuilderCollection Constraints => ReinterpretCast.To<PostgreSqlConstraintBuilderCollection>( base.Constraints );

    public new PostgreSqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
