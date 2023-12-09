using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinitionProvider
{
    [Pure]
    IEnumerable<ISqlColumnTypeDefinition> GetAll();

    [Pure]
    ISqlColumnTypeDefinition GetByDataType(ISqlDataType dataType);

    [Pure]
    ISqlColumnTypeDefinition GetByType(Type type);

    ISqlColumnTypeDefinitionProvider RegisterDefinition<T>(ISqlColumnTypeDefinition<T> definition)
        where T : notnull;
}
