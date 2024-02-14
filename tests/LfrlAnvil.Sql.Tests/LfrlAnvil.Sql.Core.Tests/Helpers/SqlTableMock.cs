using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlTableMock : SqlTable
{
    public SqlTableMock(SqlSchema schema, SqlTableBuilder builder)
        : base(
            schema,
            builder,
            new SqlColumnCollectionMock( builder.Columns ),
            new SqlConstraintCollectionMock( builder.Constraints ) ) { }
}
