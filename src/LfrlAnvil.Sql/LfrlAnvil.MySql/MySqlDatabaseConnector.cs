using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlDatabaseConnector : ISqlDatabaseConnector<MySqlConnection>, ISqlDatabaseConnector<DbConnection>
{
    private readonly string _connectionString;
    private readonly MySqlConnectionStringBuilder _connectionStringBuilder;
    private readonly DbConnectionEventHandler _eventHandler;
    private MySqlDatabase? _database;

    internal MySqlDatabaseConnector(MySqlConnectionStringBuilder connectionStringBuilder, DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _connectionString = connectionStringBuilder.ToString();
        _connectionStringBuilder = connectionStringBuilder;
        _eventHandler = eventHandler;
    }

    public MySqlDatabase Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    SqlDatabase ISqlDatabaseConnector<MySqlConnection>.Database => Database;
    SqlDatabase ISqlDatabaseConnector<DbConnection>.Database => Database;
    ISqlDatabase ISqlDatabaseConnector.Database => Database;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlConnection Connect()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<MySqlConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDatabase(MySqlDatabase database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Connector, this );
        _database = database;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private MySqlConnection CreateConnection()
    {
        var result = new MySqlConnection( _connectionString );
        _eventHandler.Attach( result );
        return result;
    }

    [Pure]
    DbConnection ISqlDatabaseConnector<DbConnection>.Connect()
    {
        return Connect();
    }

    [Pure]
    async ValueTask<DbConnection> ISqlDatabaseConnector<DbConnection>.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    IDbConnection ISqlDatabaseConnector.Connect()
    {
        return Connect();
    }

    [Pure]
    async ValueTask<IDbConnection> ISqlDatabaseConnector.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }
}
