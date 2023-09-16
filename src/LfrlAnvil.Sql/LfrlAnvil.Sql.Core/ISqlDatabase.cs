using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

public interface ISqlDatabase : IDisposable
{
    ISqlSchemaCollection Schemas { get; }
    ISqlDataTypeProvider DataTypes { get; }
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    ISqlNodeInterpreterFactory NodeInterpreterFactory { get; }
    Version Version { get; }

    [Pure]
    IDbConnection Connect();

    [Pure]
    SqlDatabaseVersionRecord[] GetRegisteredVersions();
}
