using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlSchemaCollectionMock : SqlSchemaCollection
{
    public SqlSchemaCollectionMock(SqlSchemaBuilderCollectionMock source)
        : base( source ) { }

    [Pure]
    public new SqlObjectEnumerator<SqlSchema, SqlSchemaMock> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqlSchemaMock>();
    }

    [Pure]
    protected override SqlSchemaMock CreateSchema(SqlSchemaBuilder builder)
    {
        return new SqlSchemaMock( Database, builder );
    }

    [Pure]
    protected override SqlSchemaBuilder GetSchemaFromUnknown(SqlObjectBuilder builder)
    {
        if ( builder is not SqlUnknownObjectBuilderMock u || u.UseDefaultImplementation )
            return base.GetSchemaFromUnknown( builder );

        return u.Table.Schema;
    }
}
