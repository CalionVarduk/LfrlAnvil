using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Internal;

[Pure]
public delegate TResult SqlDefaultObjectNameProviderCreator<out TResult>(string serverVersion, string defaultSchemaName)
    where TResult : ISqlDefaultObjectNameProvider;
