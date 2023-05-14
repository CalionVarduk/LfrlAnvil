﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
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
        _defaultAny = new SqliteColumnTypeDefinitionObject( this );

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

    public SqliteColumnTypeDefinitionProvider RegisterDefinition<T>(SqliteColumnTypeDefinition<T> definition)
        where T : notnull
    {
        EnsureTypeMutability( typeof( T ) );
        _definitionsByType[typeof( T )] = definition;
        return this;
    }

    [Pure]
    public IEnumerable<SqliteColumnTypeDefinition> GetAll()
    {
        return _definitionsByType.Values;
    }

    [Pure]
    public SqliteColumnTypeDefinition GetDefaultForDataType(SqliteDataType dataType)
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
        return _definitionsByType[type];
    }

    [Pure]
    internal SqliteColumnTypeDefinition? TryGetByType(Type type)
    {
        return _definitionsByType.GetValueOrDefault( type );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EnsureTypeMutability(Type type)
    {
        if ( type == _defaultInteger.RuntimeType ||
            type == _defaultReal.RuntimeType ||
            type == _defaultText.RuntimeType ||
            type == _defaultBlob.RuntimeType ||
            type == _defaultAny.RuntimeType )
            throw new InvalidOperationException( ExceptionResources.DefaultTypeDefinitionCannotBeOverriden( type ) );
    }

    [Pure]
    IEnumerable<ISqlColumnTypeDefinition> ISqlColumnTypeDefinitionProvider.GetAll()
    {
        return GetAll();
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetDefaultForDataType(ISqlDataType dataType)
    {
        return GetDefaultForDataType( SqliteHelpers.CastOrThrow<SqliteDataType>( dataType ) );
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByType(Type type)
    {
        return GetByType( type );
    }

    ISqlColumnTypeDefinitionProvider ISqlColumnTypeDefinitionProvider.RegisterDefinition<T>(ISqlColumnTypeDefinition<T> definition)
    {
        return RegisterDefinition( SqliteHelpers.CastOrThrow<SqliteColumnTypeDefinition<T>>( definition ) );
    }
}
