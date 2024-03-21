using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Internal;

[Pure]
public delegate TResult SqlNodeInterpreterFactoryCreator<in TDataTypeProvider, in TColumnTypeDefinitionProvider, out TResult>(
    string serverVersion,
    string defaultSchemaName,
    TDataTypeProvider dataTypes,
    TColumnTypeDefinitionProvider typeDefinitions)
    where TDataTypeProvider : ISqlDataTypeProvider
    where TColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
    where TResult : ISqlNodeInterpreterFactory;
