using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinitionProvider
{
    [Pure]
    IEnumerable<ISqlColumnTypeDefinition> GetAll();

    [Pure]
    ISqlColumnTypeDefinition GetDefaultForDataType(ISqlDataType dataType);

    [Pure]
    ISqlColumnTypeDefinition GetByType(Type type);

    ISqlColumnTypeDefinitionProvider RegisterDefinition<T, TBase>(
        Func<ISqlColumnTypeDefinition<TBase>, ISqlColumnTypeDefinition<T>> factory)
        where TBase : notnull
        where T : notnull;
}
