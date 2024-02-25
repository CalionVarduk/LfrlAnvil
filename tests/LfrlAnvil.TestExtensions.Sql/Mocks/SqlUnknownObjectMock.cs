using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlUnknownObjectMock : SqlConstraint
{
    public SqlUnknownObjectMock(SqlTableMock table, SqlUnknownObjectBuilderMock builder)
        : base( table, builder ) { }
}
