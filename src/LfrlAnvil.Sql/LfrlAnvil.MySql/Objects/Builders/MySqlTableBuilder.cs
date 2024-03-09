using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlTableBuilder : SqlTableBuilder
{
    internal MySqlTableBuilder(MySqlSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new MySqlColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new MySqlConstraintBuilderCollection() ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlSchemaBuilder Schema => ReinterpretCast.To<MySqlSchemaBuilder>( base.Schema );
    public new MySqlColumnBuilderCollection Columns => ReinterpretCast.To<MySqlColumnBuilderCollection>( base.Columns );
    public new MySqlConstraintBuilderCollection Constraints => ReinterpretCast.To<MySqlConstraintBuilderCollection>( base.Constraints );

    public new MySqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
