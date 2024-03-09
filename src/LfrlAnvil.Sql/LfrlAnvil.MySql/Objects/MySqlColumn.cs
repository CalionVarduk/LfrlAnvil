using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlColumn : SqlColumn
{
    internal MySqlColumn(MySqlTable table, MySqlColumnBuilder builder)
        : base( table, builder ) { }

    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );

    [Pure]
    public new SqlIndexColumn<MySqlColumn> Asc()
    {
        return SqlIndexColumn.CreateAsc( this );
    }

    [Pure]
    public new SqlIndexColumn<MySqlColumn> Desc()
    {
        return SqlIndexColumn.CreateDesc( this );
    }
}
