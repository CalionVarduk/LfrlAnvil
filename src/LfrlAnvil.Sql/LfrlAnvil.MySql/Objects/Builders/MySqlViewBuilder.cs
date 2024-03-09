using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlViewBuilder : SqlViewBuilder
{
    internal MySqlViewBuilder(
        MySqlSchemaBuilder schema,
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
        : base( schema, name, source, referencedObjects ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlSchemaBuilder Schema => ReinterpretCast.To<MySqlSchemaBuilder>( base.Schema );

    public new MySqlViewBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
