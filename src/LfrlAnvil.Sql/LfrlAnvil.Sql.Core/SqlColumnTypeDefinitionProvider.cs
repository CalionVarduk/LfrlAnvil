using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql;

public abstract class SqlColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
{
    private static readonly MethodInfo CreateEnumTypeDefinitionGenericMethod = typeof( SqlColumnTypeDefinitionProvider )
        .GetMethod( nameof( CreateEnumTypeDefinition ), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly )!;

    private readonly Dictionary<Type, SqlColumnTypeDefinition> _definitionsByType;
    protected bool IsLocked;

    protected SqlColumnTypeDefinitionProvider(SqlColumnTypeDefinitionProviderBuilder builder)
    {
        Dialect = builder.Dialect;
        _definitionsByType = builder.Definitions;
        IsLocked = false;
    }

    public SqlDialect Dialect { get; }

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
    public SqlColumnTypeDefinition? TryGetByType(Type type)
    {
        if ( _definitionsByType.TryGetValue( type, out var definition ) )
            return definition;

        if ( IsLocked )
            return null;

        if ( type.IsEnum )
        {
            var underlyingType = type.GetEnumUnderlyingType();
            if ( _definitionsByType.TryGetValue( underlyingType, out definition ) )
            {
                var method = CreateEnumTypeDefinitionGenericMethod.MakeGenericMethod( type, underlyingType );
                definition = ReinterpretCast.To<SqlColumnTypeDefinition>( method.Invoke( this, new object[] { definition } ) );
                Assume.IsNotNull( definition );
                Assume.Equals( definition.RuntimeType, type );
                _definitionsByType.Add( definition.RuntimeType, definition );
                return definition;
            }
        }

        definition = TryCreateUnknownTypeDefinition( type );
        if ( definition is not null )
        {
            Assume.Equals( definition.RuntimeType, type );
            _definitionsByType.Add( definition.RuntimeType, definition );
        }

        return definition;
    }

    [Pure]
    protected abstract SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
        where TEnum : struct, Enum
        where TUnderlying : unmanaged;

    [Pure]
    protected virtual SqlColumnTypeDefinition? TryCreateUnknownTypeDefinition(Type type)
    {
        return null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Lock()
    {
        IsLocked = true;
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
