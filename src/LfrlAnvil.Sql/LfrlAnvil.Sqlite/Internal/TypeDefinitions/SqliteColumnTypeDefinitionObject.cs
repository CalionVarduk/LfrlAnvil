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
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionObject : SqliteColumnTypeDefinition<object>
{
    private readonly SqliteColumnTypeDefinitionProvider _provider;

    internal SqliteColumnTypeDefinitionObject(SqliteColumnTypeDefinitionProvider provider, SqliteColumnTypeDefinitionByteArray @base)
        : base( SqliteDataType.Any, @base.DefaultValue.GetValue(), static (reader, ordinal) => reader.GetValue( ordinal ) )
    {
        _provider = provider;
    }

    [Pure]
    public override string ToDbLiteral(object value)
    {
        var definition = _provider.TryGetByType( value.GetType() );
        var result = definition is not null && definition.RuntimeType != typeof( object ) ? definition.TryToDbLiteral( value ) : null;
        if ( result is not null )
            return result;

        throw new ArgumentException( ExceptionResources.ValueCannotBeConvertedToDbLiteral( typeof( object ) ), nameof( value ) );
    }

    [Pure]
    public override object ToParameterValue(object value)
    {
        var definition = _provider.TryGetByType( value.GetType() );
        return definition is not null && definition.RuntimeType != typeof( object )
            ? definition.TryToParameterValue( value ) ?? value
            : value;
    }

    public override void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        parameter.IsNullable = isNullable;
        parameter.ResetDbType();
        if ( isNullable )
            parameter.DbType = DbType.Object;
    }
}
