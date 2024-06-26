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
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sqlite.Internal.TypeDefinitions;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumnTypeDefinitionProvider : SqlColumnTypeDefinitionProvider
{
    private readonly ReadOnlyArray<SqlColumnTypeDefinition> _defaultDefinitions;

    internal SqliteColumnTypeDefinitionProvider(SqliteColumnTypeDefinitionProviderBuilder builder)
        : base( builder )
    {
        var defaultAny = new SqliteColumnTypeDefinitionObject( this, builder.DefaultBlob );
        _defaultDefinitions = new SqlColumnTypeDefinition[]
        {
            defaultAny, builder.DefaultInteger, builder.DefaultReal, builder.DefaultText, builder.DefaultBlob
        };

        TryAddDefinition( defaultAny );
    }

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _defaultDefinitions.GetUnderlyingArray();
    }

    /// <inheritdoc cref="GetByDataType(ISqlDataType)" />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlColumnTypeDefinition GetByDataType(SqliteDataType type)
    {
        var index = ( int )type.Value;
        return _defaultDefinitions[index];
    }

    /// <inheritdoc />
    [Pure]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return GetByDataType( SqlHelpers.CastOrThrow<SqliteDataType>( Dialect, type ) );
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
        return new SqliteColumnTypeEnumDefinition<TEnum, TUnderlying>(
            ReinterpretCast.To<SqliteColumnTypeDefinition<TUnderlying>>( underlyingTypeDefinition ) );
    }
}
