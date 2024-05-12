using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql;

/// <inheritdoc />
public abstract class SqlColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
{
    private static readonly MethodInfo CreateEnumTypeDefinitionGenericMethod = typeof( SqlColumnTypeDefinitionProvider )
        .GetMethod( nameof( CreateEnumTypeDefinition ), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly )!;

    private readonly Dictionary<Type, SqlColumnTypeDefinition> _definitionsByType;

    /// <summary>
    /// Specifies that new registrations are disabled for this provider.
    /// </summary>
    protected bool IsLocked;

    /// <summary>
    /// Creates a new <see cref="SqlColumnTypeDefinitionProvider"/> instance.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    protected SqlColumnTypeDefinitionProvider(SqlColumnTypeDefinitionProviderBuilder builder)
    {
        Dialect = builder.Dialect;
        _definitionsByType = new Dictionary<Type, SqlColumnTypeDefinition>( builder.Definitions );
        IsLocked = false;
    }

    /// <inheritdoc />
    public SqlDialect Dialect { get; }

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProvider.GetTypeDefinitions()" />
    [Pure]
    public IReadOnlyCollection<SqlColumnTypeDefinition> GetTypeDefinitions()
    {
        return _definitionsByType.Values;
    }

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProvider.GetDataTypeDefinitions()" />
    [Pure]
    public abstract IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions();

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProvider.GetByType(Type)" />
    [Pure]
    public SqlColumnTypeDefinition GetByType(Type type)
    {
        return TryGetByType( type ) ?? throw new KeyNotFoundException( ExceptionResources.MissingColumnTypeDefinition( type ) );
    }

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProvider.TryGetByType(Type)" />
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

    /// <inheritdoc />
    [Pure]
    public bool Contains(ISqlColumnTypeDefinition definition)
    {
        var byType = TryGetByType( definition.RuntimeType );
        if ( ReferenceEquals( definition, byType ) )
            return true;

        var byDataType = GetByDataType( definition.DataType );
        return ReferenceEquals( definition, byDataType );
    }

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProvider.GetByDataType(ISqlDataType)" />
    [Pure]
    public abstract SqlColumnTypeDefinition GetByDataType(ISqlDataType type);

    /// <summary>
    /// Creates a new <see cref="SqlColumnTypeDefinition{T}"/> instance
    /// for the <typeparamref name="TEnum"/> type with <typeparamref name="TUnderlying"/> type.
    /// </summary>
    /// <param name="underlyingTypeDefinition">Column type definition associated with the underlying type.</param>
    /// <typeparam name="TEnum"><see cref="Enum"/> type.</typeparam>
    /// <typeparam name="TUnderlying">Type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
    /// <returns>New <see cref="SqlColumnTypeDefinition{T}"/> instance.</returns>
    [Pure]
    protected abstract SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
        where TEnum : struct, Enum
        where TUnderlying : unmanaged;

    /// <summary>
    /// Attempts to create a new <see cref="SqlColumnTypeDefinition"/> instance
    /// associated with the provided <paramref name="type"/> to dynamically register.
    /// </summary>
    /// <param name="type">Type to register.</param>
    /// <returns>
    /// New <see cref="SqlColumnTypeDefinition"/>
    /// or null when column type definition for the provided <paramref name="type"/> should not be dynamically registered.
    /// </returns>
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

    /// <summary>
    /// Attempts to add a new column type definition.
    /// </summary>
    /// <param name="definition">Definition to add.</param>
    /// <returns><b>true</b> when definition was added, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
