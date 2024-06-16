// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
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

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _defaultDefinitionsByTypeName.Values;
    }

    /// <inheritdoc cref="GetByDataType(ISqlDataType)" />
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

    /// <inheritdoc />
    [Pure]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return GetByDataType( SqlHelpers.CastOrThrow<MySqlDataType>( Dialect, type ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnTypeDefinition{T}"/> instance
    /// for the <typeparamref name="TEnum"/> type with <typeparamref name="TUnderlying"/> type.
    /// </summary>
    /// <param name="underlyingTypeDefinition">Column type definition associated with the underlying type.</param>
    /// <typeparam name="TEnum"><see cref="Enum"/> type.</typeparam>
    /// <typeparam name="TUnderlying">Type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
    /// <returns>New <see cref="SqlColumnTypeDefinition{T}"/> instance.</returns>
    [Pure]
    protected override SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
    {
        return new MySqlColumnTypeEnumDefinition<TEnum, TUnderlying>(
            ReinterpretCast.To<MySqlColumnTypeDefinition<TUnderlying>>( underlyingTypeDefinition ) );
    }
}
