using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlColumnCollection : SqlColumnCollection
{
    internal MySqlColumnCollection(MySqlColumnBuilderCollection source)
        : base( source ) { }

    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    [Pure]
    public new MySqlColumn Get(string name)
    {
        return ReinterpretCast.To<MySqlColumn>( base.Get( name ) );
    }

    [Pure]
    public new MySqlColumn? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlColumn>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlColumn, MySqlColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlColumn>();
    }

    protected override MySqlColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new MySqlColumn( Table, ReinterpretCast.To<MySqlColumnBuilder>( builder ) );
    }
}
