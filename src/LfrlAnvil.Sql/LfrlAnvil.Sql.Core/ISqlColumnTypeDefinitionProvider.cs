using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinitionProvider
{
    [Pure]
    IReadOnlyCollection<ISqlColumnTypeDefinition> GetTypeDefinitions();

    [Pure]
    IReadOnlyCollection<ISqlColumnTypeDefinition> GetDataTypeDefinitions();

    [Pure]
    ISqlColumnTypeDefinition GetByDataType(ISqlDataType dataType);

    [Pure]
    ISqlColumnTypeDefinition GetByType(Type type);

    [Pure]
    ISqlColumnTypeDefinition? TryGetByType(Type type);

    [Pure]
    bool Contains(ISqlColumnTypeDefinition definition);

    // TODO: remove in the future
    ISqlColumnTypeDefinitionProvider RegisterDefinition<T>(ISqlColumnTypeDefinition<T> definition)
        where T : notnull;
}
