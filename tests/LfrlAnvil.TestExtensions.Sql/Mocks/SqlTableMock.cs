using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlTableMock : SqlTable
{
    public SqlTableMock(SqlSchema schema, SqlTableBuilder builder)
        : base(
            schema,
            builder,
            new SqlColumnCollectionMock( builder.Columns ),
            new SqlConstraintCollectionMock( builder.Constraints ) ) { }

    [Pure]
    public static SqlTableMock Create<TColumnType>(
        string name,
        string[] columns,
        string[]? pkColumns = null,
        bool areColumnsNullable = false,
        string? schemaName = null)
        where TColumnType : notnull
    {
        var builder = SqlTableBuilderMock.Create<TColumnType>( name, columns, pkColumns, areColumnsNullable, schemaName );
        var db = SqlDatabaseMock.Create( builder.Database );
        var schema = schemaName is null ? db.Schemas.Default : db.Schemas.Get( schemaName );
        var table = schema.Objects.GetTable( name );
        return (SqlTableMock)table;
    }
}
