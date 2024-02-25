using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlViewMock : SqlView
{
    public SqlViewMock(SqlSchema schema, SqlViewBuilder builder)
        : base( schema, builder, new SqlViewDataFieldCollectionMock( builder.Source ) ) { }

    [Pure]
    public static SqlViewMock Create(string name, SqlQueryExpressionNode? source = null, string? schemaName = null)
    {
        var builder = SqlViewBuilderMock.Create( name, source, schemaName );
        var db = SqlDatabaseMock.Create( builder.Database );
        var schema = schemaName is null ? db.Schemas.Default : db.Schemas.Get( schemaName );
        var view = schema.Objects.GetView( name );
        return (SqlViewMock)view;
    }
}
