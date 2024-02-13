using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql;

// TODO:
// rename to Factory? then we'll have FactoryBuilder?
// also, due to enums & data types, this needs to be synchronized, thread-safe
// reader-writer lock?
// or, when db builder is active, allow for changes (builders are by design single threaded)
// and when db builder is done, then db factory will Lock() the provider
public abstract class SqlColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
{
    private readonly Dictionary<Type, SqlColumnTypeDefinition> _definitionsByType;

    protected SqlColumnTypeDefinitionProvider()
    {
        _definitionsByType = new Dictionary<Type, SqlColumnTypeDefinition>();
    }

    [Pure]
    public IReadOnlyCollection<SqlColumnTypeDefinition> GetTypeDefinitions()
    {
        return _definitionsByType.Values;
    }

    [Pure]
    public abstract IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions();

    [Pure]
    public SqlColumnTypeDefinition GetByType(Type type)
    {
        return TryGetByType( type ) ?? throw new KeyNotFoundException( ExceptionResources.MissingColumnTypeDefinition( type ) );
    }

    [Pure]
    public virtual SqlColumnTypeDefinition? TryGetByType(Type type)
    {
        return _definitionsByType.GetValueOrDefault( type );
    }

    [Pure]
    public bool Contains(ISqlColumnTypeDefinition definition)
    {
        var byType = TryGetByType( definition.RuntimeType );
        if ( ReferenceEquals( definition, byType ) )
            return true;

        var byDataType = GetByDataType( definition.DataType );
        return ReferenceEquals( definition, byDataType );
    }

    [Pure]
    public abstract SqlColumnTypeDefinition GetByDataType(ISqlDataType type);

    public ISqlColumnTypeDefinitionProvider RegisterDefinition<T>(ISqlColumnTypeDefinition<T> definition)
        where T : notnull
    {
        throw new NotImplementedException( "to be removed" );
    }

    protected bool TryAddDefinition(SqlColumnTypeDefinition definition)
    {
        return _definitionsByType.TryAdd( definition.RuntimeType, definition );
    }

    [Pure]
    IReadOnlyCollection<ISqlColumnTypeDefinition> ISqlColumnTypeDefinitionProvider.GetTypeDefinitions()
    {
        return GetTypeDefinitions();
    }

    [Pure]
    IReadOnlyCollection<ISqlColumnTypeDefinition> ISqlColumnTypeDefinitionProvider.GetDataTypeDefinitions()
    {
        return GetDataTypeDefinitions();
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByType(Type type)
    {
        return GetByType( type );
    }

    [Pure]
    ISqlColumnTypeDefinition? ISqlColumnTypeDefinitionProvider.TryGetByType(Type type)
    {
        return TryGetByType( type );
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByDataType(ISqlDataType type)
    {
        return GetByDataType( type );
    }
}
