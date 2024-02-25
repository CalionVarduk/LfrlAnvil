using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlIndexMock : SqlIndex
{
    public SqlIndexMock(SqlTable table, SqlIndexBuilder builder)
        : base( table, builder ) { }
}
