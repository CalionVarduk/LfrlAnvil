using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlPrimaryKeyMock : SqlPrimaryKey
{
    public SqlPrimaryKeyMock(SqlIndex index, SqlPrimaryKeyBuilder builder)
        : base( index, builder ) { }
}
