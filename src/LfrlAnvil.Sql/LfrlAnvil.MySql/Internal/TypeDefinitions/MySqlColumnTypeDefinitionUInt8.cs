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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionUInt8 : MySqlColumnTypeDefinition<byte>
{
    internal MySqlColumnTypeDefinitionUInt8()
        : base( MySqlDataType.UnsignedTinyInt, 0, static (reader, ordinal) => reader.GetByte( ordinal ) ) { }

    [Pure]
    public override string ToDbLiteral(byte value)
    {
        return SqlHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(byte value)
    {
        return value;
    }
}
