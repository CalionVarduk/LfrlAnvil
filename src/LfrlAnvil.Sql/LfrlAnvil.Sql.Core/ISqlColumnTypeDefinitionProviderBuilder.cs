using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public interface ISqlColumnTypeDefinitionProviderBuilder
{
    SqlDialect Dialect { get; }

    [Pure]
    bool Contains(Type type);

    ISqlColumnTypeDefinitionProviderBuilder Register(ISqlColumnTypeDefinition definition);

    [Pure]
    ISqlColumnTypeDefinitionProvider Build();
}
