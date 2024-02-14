using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlViewMock : SqlView
{
    public SqlViewMock(SqlSchema schema, SqlViewBuilder builder)
        : base( schema, builder, new SqlViewDataFieldCollectionMock( builder.Source ) ) { }
}
