using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

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

    [Pure]
    public static SqlViewBuilderMock Create(string name, SqlQueryExpressionNode? source = null, string? schemaName = null)
    {
        var db = SqlDatabaseBuilderMock.Create();
        var schema = schemaName is null ? db.Schemas.Default : db.Schemas.GetOrCreate( schemaName );
        source ??= SqlNode.RawQuery( "SELECT * FROM foo" );
        var view = schema.Objects.CreateView( name, source );
        return view;
    }
}
