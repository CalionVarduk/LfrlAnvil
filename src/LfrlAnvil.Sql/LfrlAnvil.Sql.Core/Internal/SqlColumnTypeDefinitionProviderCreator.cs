using System.Data.Common;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a delegate for creating <see cref="ISqlColumnTypeDefinitionProvider"/> instances.
/// </summary>
/// <param name="serverVersion"><see cref="DbConnection.ServerVersion"/>.</param>
/// <param name="dataTypes"><see cref="ISqlDataTypeProvider"/> instance.</param>
/// <typeparam name="TDataTypeProvider">SQL data type provider type.</typeparam>
/// <typeparam name="TResult">SQL column type definition provider type.</typeparam>
[Pure]
public delegate TResult SqlColumnTypeDefinitionProviderCreator<in TDataTypeProvider, out TResult>(
    string serverVersion,
    TDataTypeProvider dataTypes)
    where TDataTypeProvider : ISqlDataTypeProvider
    where TResult : ISqlColumnTypeDefinitionProvider;
