using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlPrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal PostgreSqlPrimaryKeyBuilder(PostgreSqlIndexBuilder index, string name)
        : base( index, name ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );
    public new PostgreSqlIndexBuilder Index => ReinterpretCast.To<PostgreSqlIndexBuilder>( base.Index );

    public new PostgreSqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new PostgreSqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
