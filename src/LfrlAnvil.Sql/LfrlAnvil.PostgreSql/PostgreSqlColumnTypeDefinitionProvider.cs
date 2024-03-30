using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql;

public sealed class PostgreSqlColumnTypeDefinitionProvider : SqlColumnTypeDefinitionProvider
{
    private readonly PostgreSqlColumnTypeDefinitionDecimal _defaultDecimal;
    private readonly PostgreSqlColumnTypeDefinitionString _defaultVarChar;
    private readonly PostgreSqlColumnTypeDefinitionObject _defaultObject;
    private readonly Dictionary<string, SqlColumnTypeDefinition> _defaultDefinitionsByTypeName;
    private readonly object _sync = new object();

    internal PostgreSqlColumnTypeDefinitionProvider(PostgreSqlColumnTypeDefinitionProviderBuilder builder)
        : base( builder )
    {
        _defaultDecimal = builder.DefaultDecimal;
        _defaultVarChar = builder.DefaultVarChar;
        _defaultObject = new PostgreSqlColumnTypeDefinitionObject( this, builder.DefaultBytea );
        _defaultDefinitionsByTypeName = builder.CreateDataTypeDefinitionsByName();
        TryAddDefinition( _defaultObject );
    }

    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _defaultDefinitionsByTypeName.Values;
    }

    [Pure]
    public SqlColumnTypeDefinition GetByDataType(PostgreSqlDataType dataType)
    {
        lock ( _sync )
        {
            if ( _defaultDefinitionsByTypeName.TryGetValue( dataType.Name, out var definition ) )
                return definition;

            switch ( dataType.Value )
            {
                case NpgsqlDbType.Numeric:
                {
                    var result = new PostgreSqlColumnTypeDefinitionDecimal( _defaultDecimal, dataType );
                    _defaultDefinitionsByTypeName[dataType.Name] = result;
                    return result;
                }
                case NpgsqlDbType.Text:
                case NpgsqlDbType.Char:
                case NpgsqlDbType.Varchar:
                {
                    var result = new PostgreSqlColumnTypeDefinitionString( _defaultVarChar, dataType );
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
        return GetByDataType( SqlHelpers.CastOrThrow<PostgreSqlDataType>( Dialect, type ) );
    }

    [Pure]
    protected override SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
    {
        return new PostgreSqlColumnTypeEnumDefinition<TEnum, TUnderlying>(
            ReinterpretCast.To<PostgreSqlColumnTypeDefinition<TUnderlying>>( underlyingTypeDefinition ) );
    }
}
