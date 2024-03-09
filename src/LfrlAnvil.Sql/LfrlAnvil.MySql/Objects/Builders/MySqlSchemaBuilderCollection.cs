using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal MySqlSchemaBuilderCollection() { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );
    public new MySqlSchemaBuilder Default => ReinterpretCast.To<MySqlSchemaBuilder>( base.Default );

    [Pure]
    public new MySqlSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.Get( name ) );
    }

    [Pure]
    public new MySqlSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.TryGet( name ) );
    }

    public new MySqlSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.Create( name ) );
    }

    public new MySqlSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, MySqlSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlSchemaBuilder>();
    }

    protected override MySqlSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new MySqlSchemaBuilder( Database, name );
    }
}
