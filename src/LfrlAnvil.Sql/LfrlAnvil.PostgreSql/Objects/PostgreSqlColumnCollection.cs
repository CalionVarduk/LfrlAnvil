using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlColumnCollection : SqlColumnCollection
{
    internal PostgreSqlColumnCollection(PostgreSqlColumnBuilderCollection source)
        : base( source ) { }

    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    [Pure]
    public new PostgreSqlColumn Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumn>( base.Get( name ) );
    }

    [Pure]
    public new PostgreSqlColumn? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumn>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlColumn, PostgreSqlColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlColumn>();
    }

    protected override PostgreSqlColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new PostgreSqlColumn( Table, ReinterpretCast.To<PostgreSqlColumnBuilder>( builder ) );
    }
}
