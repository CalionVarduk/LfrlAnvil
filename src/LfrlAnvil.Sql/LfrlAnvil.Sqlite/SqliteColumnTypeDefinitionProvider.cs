using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Internal.TypeDefinitions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
{
    private readonly SqliteColumnTypeDefinitionInt64 _defaultInteger;
    private readonly SqliteColumnTypeDefinitionDouble _defaultReal;
    private readonly SqliteColumnTypeDefinitionString _defaultText;
    private readonly SqliteColumnTypeDefinitionByteArray _defaultBlob;
    private readonly SqliteColumnTypeDefinitionObject _defaultAny;

    private readonly Dictionary<Type, SqliteColumnTypeDefinition> _definitionsByType;

    internal SqliteColumnTypeDefinitionProvider()
    {
        _defaultInteger = new SqliteColumnTypeDefinitionInt64();
        _defaultReal = new SqliteColumnTypeDefinitionDouble();
        _defaultText = new SqliteColumnTypeDefinitionString();
        _defaultBlob = new SqliteColumnTypeDefinitionByteArray();
        _defaultAny = new SqliteColumnTypeDefinitionObject( this, _defaultBlob );

        _definitionsByType = new Dictionary<Type, SqliteColumnTypeDefinition>
        {
            { typeof( bool ), new SqliteColumnTypeDefinitionBool() },
            { typeof( byte ), new SqliteColumnTypeDefinitionUInt8() },
            { typeof( sbyte ), new SqliteColumnTypeDefinitionInt8() },
            { typeof( ushort ), new SqliteColumnTypeDefinitionUInt16() },
            { typeof( short ), new SqliteColumnTypeDefinitionInt16() },
            { typeof( uint ), new SqliteColumnTypeDefinitionUInt32() },
            { typeof( int ), new SqliteColumnTypeDefinitionInt32() },
            { typeof( ulong ), new SqliteColumnTypeDefinitionUInt64() },
            { typeof( TimeSpan ), new SqliteColumnTypeDefinitionTimeSpan() },
            { typeof( long ), _defaultInteger },
            { typeof( float ), new SqliteColumnTypeDefinitionFloat() },
            { typeof( double ), _defaultReal },
            { typeof( DateTime ), new SqliteColumnTypeDefinitionDateTime() },
            { typeof( DateTimeOffset ), new SqliteColumnTypeDefinitionDateTimeOffset() },
            { typeof( DateOnly ), new SqliteColumnTypeDefinitionDateOnly() },
            { typeof( TimeOnly ), new SqliteColumnTypeDefinitionTimeOnly() },
            { typeof( decimal ), new SqliteColumnTypeDefinitionDecimal() },
            { typeof( char ), new SqliteColumnTypeDefinitionChar() },
            { typeof( string ), _defaultText },
            { typeof( Guid ), new SqliteColumnTypeDefinitionGuid() },
            { typeof( byte[] ), _defaultBlob },
            { typeof( object ), _defaultAny }
        };
    }

    public SqlDialect Dialect => SqliteDialect.Instance;

    public SqliteColumnTypeDefinitionProvider RegisterDefinition<T>(SqliteColumnTypeDefinition<T> definition)
        where T : notnull
    {
        _definitionsByType[typeof( T )] = definition;
        return this;
    }

    [Pure]
    public IReadOnlyCollection<SqliteColumnTypeDefinition> GetTypeDefinitions()
    {
        return _definitionsByType.Values;
    }

    [Pure]
    public IReadOnlyCollection<SqliteColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return new SqliteColumnTypeDefinition[] { _defaultInteger, _defaultReal, _defaultText, _defaultBlob, _defaultAny };
    }

    [Pure]
    public SqliteColumnTypeDefinition GetByDataType(SqliteDataType dataType)
    {
        return dataType.Value switch
        {
            SqliteType.Integer => _defaultInteger,
            SqliteType.Real => _defaultReal,
            SqliteType.Text => _defaultText,
            SqliteType.Blob => _defaultBlob,
            _ => _defaultAny
        };
    }

    [Pure]
    public SqliteColumnTypeDefinition GetByType(Type type)
    {
        return TryGetByType( type ) ?? throw new KeyNotFoundException( ExceptionResources.MissingColumnTypeDefinition( type ) );
    }

    [Pure]
    public SqliteColumnTypeDefinition? TryGetByType(Type type)
    {
        if ( _definitionsByType.TryGetValue( type, out var result ) )
            return result;

        if ( type.IsEnum )
        {
            var underlyingType = type.GetEnumUnderlyingType();
            if ( ! _definitionsByType.TryGetValue( underlyingType, out var baseDefinition ) )
                return null;

            var definitionType = typeof( SqliteColumnTypeEnumDefinition<,> ).MakeGenericType( type, underlyingType );
            var definitionTypeCtor = definitionType.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic )[0];
            var definition = ReinterpretCast.To<SqliteColumnTypeDefinition>( definitionTypeCtor.Invoke( new object[] { baseDefinition } ) );
            _definitionsByType.Add( type, definition );
            return definition;
        }

        return null;
    }

    [Pure]
    public bool Contains(SqliteColumnTypeDefinition definition)
    {
        return ReferenceEquals( TryGetByType( definition.RuntimeType ), definition ) ||
            ReferenceEquals( GetByDataType( definition.DataType ), definition );
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
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByDataType(ISqlDataType dataType)
    {
        return GetByDataType( SqliteHelpers.CastOrThrow<SqliteDataType>( dataType ) );
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
    bool ISqlColumnTypeDefinitionProvider.Contains(ISqlColumnTypeDefinition definition)
    {
        return definition is SqliteColumnTypeDefinition d && Contains( d );
    }
}
