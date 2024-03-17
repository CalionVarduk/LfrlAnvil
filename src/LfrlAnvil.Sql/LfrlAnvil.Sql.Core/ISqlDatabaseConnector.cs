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
    ValueTask<IDbConnection> ConnectAsync(CancellationToken cancellationToken = default);
}

public interface ISqlDatabaseConnector<TConnection> : ISqlDatabaseConnector
    where TConnection : DbConnection
{
    new SqlDatabase Database { get; }

    [Pure]
    new TConnection Connect();

    [Pure]
    new ValueTask<TConnection> ConnectAsync(CancellationToken cancellationToken = default);
}
