using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlColumnTypeDefinitionProvider : ISqlColumnTypeDefinitionProvider
{
    private readonly MySqlColumnTypeDefinitionDecimal _defaultDecimal;
    private readonly MySqlColumnTypeDefinitionString _defaultText;
    private readonly MySqlColumnTypeDefinitionByteArray _defaultBlob;
    private readonly MySqlColumnTypeDefinitionObject _defaultObject;
    private readonly Dictionary<string, MySqlColumnTypeDefinition> _defaultDefinitionsByTypeName;
    private readonly Dictionary<Type, MySqlColumnTypeDefinition> _definitionsByType;

    internal MySqlColumnTypeDefinitionProvider(MySqlDataTypeProvider dataTypeProvider)
    {
        var defaultBool = new MySqlColumnTypeDefinitionBool();
        var defaultTinyInt = new MySqlColumnTypeDefinitionInt8();
        var defaultUnsignedTinyInt = new MySqlColumnTypeDefinitionUInt8();
        var defaultSmallInt = new MySqlColumnTypeDefinitionInt16();
        var defaultUnsignedSmallInt = new MySqlColumnTypeDefinitionUInt16();
        var defaultInt = new MySqlColumnTypeDefinitionInt32();
        var defaultUnsignedInt = new MySqlColumnTypeDefinitionUInt32();
        var defaultBigInt = new MySqlColumnTypeDefinitionInt64();
        var defaultUnsignedBigInt = new MySqlColumnTypeDefinitionUInt64();
        var defaultFloat = new MySqlColumnTypeDefinitionFloat();
        var defaultDouble = new MySqlColumnTypeDefinitionDouble();
        _defaultDecimal = new MySqlColumnTypeDefinitionDecimal();
        _defaultText = new MySqlColumnTypeDefinitionString();
        var defaultChar = new MySqlColumnTypeDefinitionString( _defaultText, MySqlDataType.Char );
        var defaultVarChar = new MySqlColumnTypeDefinitionString( _defaultText, MySqlDataType.VarChar );
        _defaultBlob = new MySqlColumnTypeDefinitionByteArray();
        var defaultBinary = new MySqlColumnTypeDefinitionByteArray( _defaultBlob, MySqlDataType.Binary );
        var defaultVarBinary = new MySqlColumnTypeDefinitionByteArray( _defaultBlob, MySqlDataType.VarBinary );
        var defaultDate = new MySqlColumnTypeDefinitionDateOnly();
        var defaultTime = new MySqlColumnTypeDefinitionTimeOnly();
        var defaultDateTime = new MySqlColumnTypeDefinitionDateTime();
        _defaultObject = new MySqlColumnTypeDefinitionObject( this, _defaultBlob );

        _defaultDefinitionsByTypeName = new Dictionary<string, MySqlColumnTypeDefinition>( comparer: StringComparer.OrdinalIgnoreCase )
        {
            { defaultBool.DataType.Name, defaultBool },
            { defaultTinyInt.DataType.Name, defaultTinyInt },
            { defaultUnsignedTinyInt.DataType.Name, defaultUnsignedTinyInt },
            { defaultSmallInt.DataType.Name, defaultSmallInt },
            { defaultUnsignedSmallInt.DataType.Name, defaultUnsignedSmallInt },
            { defaultInt.DataType.Name, defaultInt },
            { defaultUnsignedInt.DataType.Name, defaultUnsignedInt },
            { defaultBigInt.DataType.Name, defaultBigInt },
            { defaultUnsignedBigInt.DataType.Name, defaultUnsignedBigInt },
            { defaultFloat.DataType.Name, defaultFloat },
            { defaultDouble.DataType.Name, defaultDouble },
            { _defaultDecimal.DataType.Name, _defaultDecimal },
            { _defaultText.DataType.Name, _defaultText },
            { defaultChar.DataType.Name, defaultChar },
            { defaultVarChar.DataType.Name, defaultVarChar },
            { _defaultBlob.DataType.Name, _defaultBlob },
            { defaultBinary.DataType.Name, defaultBinary },
            { defaultVarBinary.DataType.Name, defaultVarBinary },
            { defaultDate.DataType.Name, defaultDate },
            { defaultTime.DataType.Name, defaultTime },
            { defaultDateTime.DataType.Name, defaultDateTime }
        };

        _definitionsByType = new Dictionary<Type, MySqlColumnTypeDefinition>
        {
            { typeof( bool ), defaultBool },
            { typeof( byte ), defaultUnsignedTinyInt },
            { typeof( sbyte ), defaultTinyInt },
            { typeof( ushort ), defaultUnsignedSmallInt },
            { typeof( short ), defaultSmallInt },
            { typeof( uint ), defaultUnsignedInt },
            { typeof( int ), defaultInt },
            { typeof( ulong ), defaultUnsignedBigInt },
            { typeof( TimeSpan ), new MySqlColumnTypeDefinitionTimeSpan() },
            { typeof( long ), defaultBigInt },
            { typeof( float ), defaultFloat },
            { typeof( double ), defaultDouble },
            { typeof( DateTime ), defaultDateTime },
            { typeof( DateTimeOffset ), new MySqlColumnTypeDefinitionDateTimeOffset() },
            { typeof( DateOnly ), defaultDate },
            { typeof( TimeOnly ), defaultTime },
            { typeof( decimal ), _defaultDecimal },
            { typeof( char ), new MySqlColumnTypeDefinitionChar() },
            { typeof( string ), _defaultText },
            { typeof( Guid ), new MySqlColumnTypeDefinitionGuid( dataTypeProvider ) },
            { typeof( byte[] ), _defaultBlob },
            { typeof( object ), _defaultObject }
        };
    }

    public MySqlColumnTypeDefinitionProvider RegisterDefinition<T>(MySqlColumnTypeDefinition<T> definition)
        where T : notnull
    {
        _definitionsByType[typeof( T )] = definition;
        return this;
    }

    [Pure]
    public IEnumerable<MySqlColumnTypeDefinition> GetAll()
    {
        return _definitionsByType.Values.Union( _defaultDefinitionsByTypeName.Values.Append( _defaultObject ) );
    }

    [Pure]
    public MySqlColumnTypeDefinition GetByDataType(MySqlDataType dataType)
    {
        if ( _defaultDefinitionsByTypeName.TryGetValue( dataType.Name, out var definition ) )
            return definition;

        switch ( dataType.Value )
        {
            case MySqlDbType.Decimal:
            case MySqlDbType.NewDecimal:
            {
                var result = new MySqlColumnTypeDefinitionDecimal( _defaultDecimal, dataType );
                _defaultDefinitionsByTypeName[dataType.Name] = result;
                return result;
            }
            case MySqlDbType.String:
            case MySqlDbType.VarChar:
            case MySqlDbType.VarString:
            case MySqlDbType.TinyText:
            case MySqlDbType.Text:
            case MySqlDbType.MediumText:
            case MySqlDbType.LongText:
            {
                var result = new MySqlColumnTypeDefinitionString( _defaultText, dataType );
                _defaultDefinitionsByTypeName[dataType.Name] = result;
                return result;
            }
            case MySqlDbType.Binary:
            case MySqlDbType.VarBinary:
            case MySqlDbType.TinyBlob:
            case MySqlDbType.Blob:
            case MySqlDbType.MediumBlob:
            case MySqlDbType.LongBlob:
            {
                var result = new MySqlColumnTypeDefinitionByteArray( _defaultBlob, dataType );
                _defaultDefinitionsByTypeName[dataType.Name] = result;
                return result;
            }
        }

        return _defaultObject;
    }

    [Pure]
    public MySqlColumnTypeDefinition GetByType(Type type)
    {
        return TryGetByType( type ) ?? throw new KeyNotFoundException( ExceptionResources.MissingColumnTypeDefinition( type ) );
    }

    [Pure]
    internal MySqlColumnTypeDefinition? TryGetByType(Type type)
    {
        if ( _definitionsByType.TryGetValue( type, out var result ) )
            return result;

        if ( type.IsEnum )
        {
            var underlyingType = type.GetEnumUnderlyingType();
            if ( ! _definitionsByType.TryGetValue( underlyingType, out var baseDefinition ) )
                return null;

            var definitionType = typeof( MySqlColumnTypeEnumDefinition<,> ).MakeGenericType( type, underlyingType );
            var definitionTypeCtor = definitionType.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic )[0];
            var definition = (MySqlColumnTypeDefinition)definitionTypeCtor.Invoke( new object[] { baseDefinition } );
            _definitionsByType.Add( type, definition );
            return definition;
        }

        return null;
    }

    [Pure]
    IEnumerable<ISqlColumnTypeDefinition> ISqlColumnTypeDefinitionProvider.GetAll()
    {
        return GetAll();
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByDataType(ISqlDataType dataType)
    {
        return GetByDataType( MySqlHelpers.CastOrThrow<MySqlDataType>( dataType ) );
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByType(Type type)
    {
        return GetByType( type );
    }

    ISqlColumnTypeDefinitionProvider ISqlColumnTypeDefinitionProvider.RegisterDefinition<T>(ISqlColumnTypeDefinition<T> definition)
    {
        return RegisterDefinition( MySqlHelpers.CastOrThrow<MySqlColumnTypeDefinition<T>>( definition ) );
    }
}
