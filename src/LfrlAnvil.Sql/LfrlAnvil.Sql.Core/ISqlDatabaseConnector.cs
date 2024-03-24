using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql;

public interface ISqlDatabaseConnector
{
    ISqlDatabase Database { get; }

    [Pure]
    IDbConnection Connect();

    [Pure]
    IDbConnection Connect(string options);

    [Pure]
    ValueTask<IDbConnection> ConnectAsync(CancellationToken cancellationToken = default);

    [Pure]
    ValueTask<IDbConnection> ConnectAsync(string options, CancellationToken cancellationToken = default);
}

public interface ISqlDatabaseConnector<TConnection> : ISqlDatabaseConnector
    where TConnection : DbConnection
{
    new SqlDatabase Database { get; }

    [Pure]
    new TConnection Connect();

    [Pure]
    new TConnection Connect(string options);

    [Pure]
    new ValueTask<TConnection> ConnectAsync(CancellationToken cancellationToken = default);

    [Pure]
    new ValueTask<TConnection> ConnectAsync(string options, CancellationToken cancellationToken = default);
}
