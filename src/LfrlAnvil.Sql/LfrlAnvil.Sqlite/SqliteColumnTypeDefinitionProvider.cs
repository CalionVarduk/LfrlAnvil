using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
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
            { typeof( byte ), new SqliteColumnTypeDefinitionUInt8( _defaultInteger ) },
            { typeof( sbyte ), new SqliteColumnTypeDefinitionInt8( _defaultInteger ) },
            { typeof( ushort ), new SqliteColumnTypeDefinitionUInt16( _defaultInteger ) },
            { typeof( short ), new SqliteColumnTypeDefinitionInt16( _defaultInteger ) },
            { typeof( uint ), new SqliteColumnTypeDefinitionUInt32( _defaultInteger ) },
            { typeof( int ), new SqliteColumnTypeDefinitionInt32( _defaultInteger ) },
            { typeof( ulong ), new SqliteColumnTypeDefinitionUInt64( _defaultInteger ) },
            { typeof( TimeSpan ), new SqliteColumnTypeDefinitionTimeSpan( _defaultInteger ) },
            { typeof( long ), _defaultInteger },
            { typeof( float ), new SqliteColumnTypeDefinitionFloat( _defaultReal ) },
            { typeof( double ), _defaultReal },
            { typeof( DateTime ), new SqliteColumnTypeDefinitionDateTime( _defaultText ) },
            { typeof( DateTimeOffset ), new SqliteColumnTypeDefinitionDateTimeOffset( _defaultText ) },
            { typeof( DateOnly ), new SqliteColumnTypeDefinitionDateOnly( _defaultText ) },
            { typeof( TimeOnly ), new SqliteColumnTypeDefinitionTimeOnly( _defaultText ) },
            { typeof( decimal ), new SqliteColumnTypeDefinitionDecimal( _defaultText ) },
            { typeof( char ), new SqliteColumnTypeDefinitionChar() },
            { typeof( string ), _defaultText },
            { typeof( Guid ), new SqliteColumnTypeDefinitionGuid( _defaultBlob ) },
            { typeof( byte[] ), _defaultBlob },
            { typeof( object ), _defaultAny }
        };
    }

    public SqliteColumnTypeDefinitionProvider RegisterDefinition<T, TBase>(
        Func<SqliteColumnTypeDefinition<TBase>, SqliteColumnTypeDefinition<T>> factory)
        where TBase : notnull
        where T : notnull
    {
        return RegisterDefinitionCore( factory );
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

    private SqliteColumnTypeDefinitionProvider RegisterDefinitionCore<T, TBase>(
        Func<SqliteColumnTypeDefinition<TBase>, ISqlColumnTypeDefinition<T>> factory)
        where T : notnull
        where TBase : notnull
    {
        EnsureTypeMutability( typeof( T ) );
        var @base = (SqliteColumnTypeDefinition<TBase>)GetByType( typeof( TBase ) );

        var result = SqliteHelpers.CastOrThrow<SqliteColumnTypeDefinition<T>>( factory( @base ) );
        if ( ! result.DbType.IsCompatibleWith( @base.DbType ) )
        {
            throw new InvalidOperationException(
                ExceptionResources.ExtendedTypeDefinitionIsIncompatibleWithBase(
                    typeof( TBase ),
                    @base.DbType,
                    typeof( T ),
                    result.DbType ) );
        }

        _definitionsByType[result.RuntimeType] = result;
        return this;
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
        return GetDefaultForDataType( (SqliteDataType)dataType );
    }

    [Pure]
    ISqlColumnTypeDefinition ISqlColumnTypeDefinitionProvider.GetByType(Type type)
    {
        return GetByType( type );
    }

    ISqlColumnTypeDefinitionProvider ISqlColumnTypeDefinitionProvider.RegisterDefinition<T, TBase>(
        Func<ISqlColumnTypeDefinition<TBase>, ISqlColumnTypeDefinition<T>> factory)
    {
        return RegisterDefinitionCore( factory );
    }
}
