using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal MySqlForeignKeyBuilder(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    public new MySqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    public new MySqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    public new MySqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
