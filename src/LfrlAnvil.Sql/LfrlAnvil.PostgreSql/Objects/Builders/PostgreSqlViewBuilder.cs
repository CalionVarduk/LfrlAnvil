using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlViewBuilder : SqlViewBuilder
{
    internal PostgreSqlViewBuilder(
        PostgreSqlSchemaBuilder schema,
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
        : base( schema, name, source, referencedObjects ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlSchemaBuilder Schema => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Schema );

    public new PostgreSqlViewBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
