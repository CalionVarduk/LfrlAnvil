using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseMock : SqlDatabase
{
    internal SqlDatabaseMock(
        SqlDatabaseBuilderMock builder,
        SqlDatabaseConnectorMock connector,
        Version version,
        Func<IEnumerable<SqlDatabaseVersionRecord>>? versionRecordsProvider = null)
        : base(
            builder,
            new SqlSchemaCollectionMock( builder.Schemas ),
            connector,
            version,
            new SqlQueryReader<SqlDatabaseVersionRecord>(
                    builder.Dialect,
                    (_, _) => versionRecordsProvider is null
                        ? SqlQueryResult<SqlDatabaseVersionRecord>.Empty
                        : new SqlQueryResult<SqlDatabaseVersionRecord>( null, versionRecordsProvider().ToList() ) )
                .Bind( string.Empty ) )
    {
        connector.SetDatabase( this );
    }

    public new SqlSchemaCollectionMock Schemas => ReinterpretCast.To<SqlSchemaCollectionMock>( base.Schemas );
    public new SqlDatabaseConnectorMock Connector => ReinterpretCast.To<SqlDatabaseConnectorMock>( base.Connector );

    [Pure]
    public static SqlDatabaseMock Create(
        SqlDatabaseBuilderMock builder,
        Func<IEnumerable<SqlDatabaseVersionRecord>>? versionRecordsProvider = null)
    {
        var connector = new SqlDatabaseConnectorMock(
            new DbConnectionStringBuilder(),
            new DbConnectionEventHandler( ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>>.Empty ) );

        return new SqlDatabaseMock( builder, connector, new Version( "0.0.0" ), versionRecordsProvider );
    }
}
