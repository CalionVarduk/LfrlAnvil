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
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionGuid : SqliteColumnTypeDefinition<Guid>
{
    internal SqliteColumnTypeDefinitionGuid()
        : base( SqliteDataType.Blob, Guid.Empty, static (reader, ordinal) => new Guid( ( byte[] )reader.GetValue( ordinal ) ) ) { }

    [Pure]
    public override string ToDbLiteral(Guid value)
    {
        return SqlHelpers.GetDbLiteral( value.ToByteArray() );
    }

    [Pure]
    public override object ToParameterValue(Guid value)
    {
        return value.ToByteArray();
    }
}
