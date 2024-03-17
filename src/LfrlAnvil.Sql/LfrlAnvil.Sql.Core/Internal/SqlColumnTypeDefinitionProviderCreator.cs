using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Internal;

[Pure]
public delegate TResult SqlColumnTypeDefinitionProviderCreator<in TDataTypeProvider, out TResult>(
    string serverVersion,
    TDataTypeProvider dataTypes)
    where TDataTypeProvider : ISqlDataTypeProvider
    where TResult : ISqlColumnTypeDefinitionProvider;
