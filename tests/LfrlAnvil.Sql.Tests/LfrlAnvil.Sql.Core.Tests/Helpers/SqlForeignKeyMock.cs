using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlForeignKeyMock : SqlForeignKey
{
    public SqlForeignKeyMock(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }
}
