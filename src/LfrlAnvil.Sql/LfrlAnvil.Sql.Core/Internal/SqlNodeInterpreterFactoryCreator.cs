using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a delegate for creating <see cref="ISqlNodeInterpreterFactory"/> instances.
/// </summary>
/// <param name="serverVersion"><see cref="DbConnection.ServerVersion"/>.</param>
/// <param name="defaultSchemaName">Name of the default DB schema.</param>
/// <param name="dataTypes"><see cref="ISqlDataTypeProvider"/> instance.</param>
/// <param name="typeDefinitions"><see cref="ISqlColumnTypeDefinitionProvider"/> instance.</param>
/// <typeparam name="TDataTypeProvider">SQL data type provider type.</typeparam>
/// <typeparam name="TColumnTypeDefinitionProvider">SQL column type definition provider type.</typeparam>
/// <typeparam name="TResult">SQL node interpreter factory type.</typeparam>
[Pure]
public delegate TResult SqlNodeInterpreterFactoryCreator<in TDataTypeProvider, in TColumnTypeDefinitionProvider, out TResult>(
    string serverVersion,
    string defaultSchemaName,
    TDataTypeProvider dataTypes,
    TColumnTypeDefinitionProvider typeDefinitions)
    where TDataTypeProvider : ISqlDataTypeProvider
    where TColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
    where TResult : ISqlNodeInterpreterFactory;
