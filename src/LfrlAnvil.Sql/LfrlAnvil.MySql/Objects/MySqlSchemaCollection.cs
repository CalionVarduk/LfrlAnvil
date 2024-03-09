using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlSchemaCollection : SqlSchemaCollection
{
    internal MySqlSchemaCollection(MySqlSchemaBuilderCollection source)
        : base( source ) { }

    public new MySqlSchema Default => ReinterpretCast.To<MySqlSchema>( base.Default );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );

    [Pure]
    public new MySqlSchema Get(string name)
    {
        return ReinterpretCast.To<MySqlSchema>( base.Get( name ) );
    }

    [Pure]
    public new MySqlSchema? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlSchema>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlSchema, MySqlSchema> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlSchema>();
    }

    protected override MySqlSchema CreateSchema(SqlSchemaBuilder builder)
    {
        return new MySqlSchema( Database, ReinterpretCast.To<MySqlSchemaBuilder>( builder ) );
    }
}
