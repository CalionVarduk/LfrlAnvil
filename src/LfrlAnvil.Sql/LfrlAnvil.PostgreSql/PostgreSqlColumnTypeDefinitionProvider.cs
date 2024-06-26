﻿// Copyright 2024 Łukasz Furlepa
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
using LfrlAnvil.PostgreSql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
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

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _defaultDefinitionsByTypeName.Values;
    }

    /// <inheritdoc cref="GetByDataType(ISqlDataType)" />
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

    /// <inheritdoc />
    [Pure]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return GetByDataType( SqlHelpers.CastOrThrow<PostgreSqlDataType>( Dialect, type ) );
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
        return new PostgreSqlColumnTypeEnumDefinition<TEnum, TUnderlying>(
            ReinterpretCast.To<PostgreSqlColumnTypeDefinition<TUnderlying>>( underlyingTypeDefinition ) );
    }
}
