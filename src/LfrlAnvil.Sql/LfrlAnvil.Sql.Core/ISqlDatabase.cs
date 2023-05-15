using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

public interface ISqlDatabase
{
    ISqlSchemaCollection Schemas { get; }
    Version Version { get; }

    [Pure]
    IDbConnection Connect();

    [Pure]
    SqlDatabaseVersionRecord[] GetRegisteredVersions();
}
