using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteColumnCollection : SqlColumnCollection
{
    internal SqliteColumnCollection(SqliteColumnBuilderCollection source)
        : base( source ) { }

    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    [Pure]
    public new SqliteColumn Get(string name)
    {
        return ReinterpretCast.To<SqliteColumn>( base.Get( name ) );
    }

    [Pure]
    public new SqliteColumn? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteColumn>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlColumn, SqliteColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteColumn>();
    }

    protected override SqliteColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new SqliteColumn( Table, ReinterpretCast.To<SqliteColumnBuilder>( builder ) );
    }
}
