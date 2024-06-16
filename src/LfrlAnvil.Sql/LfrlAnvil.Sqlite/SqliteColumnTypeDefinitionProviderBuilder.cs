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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sqlite.Internal.TypeDefinitions;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public class SqliteColumnTypeDefinitionProviderBuilder : SqlColumnTypeDefinitionProviderBuilder
{
    internal readonly SqliteColumnTypeDefinitionInt64 DefaultInteger;
    internal readonly SqliteColumnTypeDefinitionDouble DefaultReal;
    internal readonly SqliteColumnTypeDefinitionString DefaultText;
    internal readonly SqliteColumnTypeDefinitionByteArray DefaultBlob;

    /// <summary>
    /// Creates a new <see cref="SqliteColumnTypeDefinitionProviderBuilder"/> instance.
    /// </summary>
    public SqliteColumnTypeDefinitionProviderBuilder()
        : base( SqliteDialect.Instance )
    {
        DefaultInteger = new SqliteColumnTypeDefinitionInt64();
        DefaultReal = new SqliteColumnTypeDefinitionDouble();
        DefaultText = new SqliteColumnTypeDefinitionString();
        DefaultBlob = new SqliteColumnTypeDefinitionByteArray();

        AddOrUpdate( DefaultInteger );
        AddOrUpdate( DefaultReal );
        AddOrUpdate( DefaultText );
        AddOrUpdate( DefaultBlob );

        AddOrUpdate( new SqliteColumnTypeDefinitionBool() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt8() );
        AddOrUpdate( new SqliteColumnTypeDefinitionInt8() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt16() );
        AddOrUpdate( new SqliteColumnTypeDefinitionInt16() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt32() );
        AddOrUpdate( new SqliteColumnTypeDefinitionInt32() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt64() );
        AddOrUpdate( new SqliteColumnTypeDefinitionTimeSpan() );
        AddOrUpdate( new SqliteColumnTypeDefinitionFloat() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDateTime() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDateTimeOffset() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDateOnly() );
        AddOrUpdate( new SqliteColumnTypeDefinitionTimeOnly() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDecimal() );
        AddOrUpdate( new SqliteColumnTypeDefinitionChar() );
        AddOrUpdate( new SqliteColumnTypeDefinitionGuid() );
    }

    /// <inheritdoc cref="SqlColumnTypeDefinitionProviderBuilder.Register(SqlColumnTypeDefinition)" />
    public new SqliteColumnTypeDefinitionProviderBuilder Register(SqlColumnTypeDefinition definition)
    {
        base.Register( definition );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public sealed override SqliteColumnTypeDefinitionProvider Build()
    {
        return new SqliteColumnTypeDefinitionProvider( this );
    }
}
