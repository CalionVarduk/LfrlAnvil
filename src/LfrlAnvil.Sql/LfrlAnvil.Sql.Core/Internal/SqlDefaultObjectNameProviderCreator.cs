using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a delegate for creating <see cref="ISqlDefaultObjectNameProvider"/> instances.
/// </summary>
/// <param name="serverVersion"><see cref="DbConnection.ServerVersion"/>.</param>
/// <param name="defaultSchemaName">Name of the default DB schema.</param>
/// <typeparam name="TResult">SQL default object name provider type.</typeparam>
[Pure]
public delegate TResult SqlDefaultObjectNameProviderCreator<out TResult>(string serverVersion, string defaultSchemaName)
    where TResult : ISqlDefaultObjectNameProvider;
