using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal PostgreSqlForeignKeyBuilder(PostgreSqlIndexBuilder originIndex, PostgreSqlIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    public new PostgreSqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new PostgreSqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new PostgreSqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    public new PostgreSqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
