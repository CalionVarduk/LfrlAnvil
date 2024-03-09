using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlColumnTypeDefinitionProvider : SqlColumnTypeDefinitionProvider
{
    private readonly MySqlColumnTypeDefinitionDecimal _defaultDecimal;
    private readonly MySqlColumnTypeDefinitionString _defaultText;
    private readonly MySqlColumnTypeDefinitionByteArray _defaultBlob;
    private readonly MySqlColumnTypeDefinitionObject _defaultObject;
    private readonly Dictionary<string, SqlColumnTypeDefinition> _defaultDefinitionsByTypeName;
    private readonly object _sync = new object();

    internal MySqlColumnTypeDefinitionProvider(MySqlColumnTypeDefinitionProviderBuilder builder)
        : base( builder )
    {
        _defaultDecimal = builder.DefaultDecimal;
        _defaultText = builder.DefaultText;
        _defaultBlob = builder.DefaultBlob;
        _defaultObject = new MySqlColumnTypeDefinitionObject( this, _defaultBlob );
        _defaultDefinitionsByTypeName = builder.CreateDataTypeDefinitionsByName();
        TryAddDefinition( _defaultObject );
    }

    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _defaultDefinitionsByTypeName.Values;
    }

    [Pure]
    public SqlColumnTypeDefinition GetByDataType(MySqlDataType dataType)
    {
        lock ( _sync )
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
        }

        return _defaultObject;
    }

    [Pure]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return GetByDataType( SqlHelpers.CastOrThrow<MySqlDataType>( Dialect, type ) );
    }

    [Pure]
    protected override SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
    {
        return new MySqlColumnTypeEnumDefinition<TEnum, TUnderlying>(
            ReinterpretCast.To<MySqlColumnTypeDefinition<TUnderlying>>( underlyingTypeDefinition ) );
    }
}
