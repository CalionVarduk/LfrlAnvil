using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlSchemaBuilder : SqlSchemaBuilder
{
    internal PostgreSqlSchemaBuilder(PostgreSqlDatabaseBuilder database, string name)
        : base( database, name, new PostgreSqlObjectBuilderCollection() ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlObjectBuilderCollection Objects => ReinterpretCast.To<PostgreSqlObjectBuilderCollection>( base.Objects );

    public new PostgreSqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
