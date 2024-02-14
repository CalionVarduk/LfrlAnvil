using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlColumnMock : SqlColumn
{
    public SqlColumnMock(SqlTable table, SqlColumnBuilder builder)
        : base( table, builder ) { }
}
