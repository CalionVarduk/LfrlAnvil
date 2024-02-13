using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlViewBuilderMock : SqlViewBuilder
{
    public SqlViewBuilderMock(
        SqlSchemaBuilderMock schema,
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
        : base( schema, name, source, referencedObjects ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlSchemaBuilderMock Schema => ReinterpretCast.To<SqlSchemaBuilderMock>( base.Schema );

    public new SqlViewBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
