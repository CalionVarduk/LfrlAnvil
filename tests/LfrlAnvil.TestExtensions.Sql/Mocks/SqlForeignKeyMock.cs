using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlForeignKeyMock : SqlForeignKey
{
    public SqlForeignKeyMock(SqlIndex originIndex, SqlIndex referencedIndex, SqlForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }
}
