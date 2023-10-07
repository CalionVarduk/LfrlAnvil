using System.Data;
using System.Data.Common;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly record struct SqlDatabaseConnectionChangeEvent(DbConnection Connection, StateChangeEventArgs StateChange);
