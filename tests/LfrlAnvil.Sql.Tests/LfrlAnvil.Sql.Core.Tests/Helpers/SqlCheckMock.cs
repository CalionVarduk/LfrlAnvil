using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlCheckMock : SqlCheck
{
    public SqlCheckMock(SqlTable table, SqlCheckBuilder builder)
        : base( table, builder ) { }
}
