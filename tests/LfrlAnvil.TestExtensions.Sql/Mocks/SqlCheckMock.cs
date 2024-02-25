using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlCheckMock : SqlCheck
{
    public SqlCheckMock(SqlTable table, SqlCheckBuilder builder)
        : base( table, builder ) { }
}
