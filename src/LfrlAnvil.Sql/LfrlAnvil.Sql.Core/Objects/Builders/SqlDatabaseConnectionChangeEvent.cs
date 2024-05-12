using System.Data;
using System.Data.Common;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an event that occurred due to a DB connection's state change.
/// </summary>
/// <param name="Connection">DB connection whose state has changed.</param>
/// <param name="StateChange">Underlying <see cref="StateChangeEventArgs"/> instance.</param>
public readonly record struct SqlDatabaseConnectionChangeEvent(DbConnection Connection, StateChangeEventArgs StateChange);
