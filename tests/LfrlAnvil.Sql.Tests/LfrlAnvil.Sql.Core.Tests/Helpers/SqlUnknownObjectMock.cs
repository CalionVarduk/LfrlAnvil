using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlUnknownObjectMock : SqlConstraint
{
    public SqlUnknownObjectMock(SqlTableMock table, SqlUnknownObjectBuilderMock builder)
        : base( table, builder ) { }
}
