using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlSchemaMock : SqlSchema
{
    public SqlSchemaMock(SqlDatabase database, SqlSchemaBuilder builder)
        : base( database, builder, new SqlObjectCollectionMock( builder.Objects ) ) { }
}
