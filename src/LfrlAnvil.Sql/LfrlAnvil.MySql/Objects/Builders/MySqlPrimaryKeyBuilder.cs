using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlPrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal MySqlPrimaryKeyBuilder(MySqlIndexBuilder index, string name)
        : base( index, name ) { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );
    public new MySqlIndexBuilder Index => ReinterpretCast.To<MySqlIndexBuilder>( base.Index );

    public new MySqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public new MySqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
