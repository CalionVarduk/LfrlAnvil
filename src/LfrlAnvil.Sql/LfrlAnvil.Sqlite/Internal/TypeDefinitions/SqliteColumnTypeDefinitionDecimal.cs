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
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionDecimal : SqliteColumnTypeDefinition<decimal>
{
    internal SqliteColumnTypeDefinitionDecimal()
        : base(
            SqliteDataType.Text,
            0m,
            static (reader, ordinal) => decimal.Parse(
                reader.GetString( ordinal ),
                NumberStyles.Number | NumberStyles.AllowExponent,
                CultureInfo.InvariantCulture ) ) { }

    [Pure]
    public override string ToDbLiteral(decimal value)
    {
        return SqliteHelpers.GetDbLiteral( value );
    }

    [Pure]
    public override object ToParameterValue(decimal value)
    {
        return value >= 0
            ? value.ToString( SqlHelpers.DecimalFormat, CultureInfo.InvariantCulture )
            : (-value).ToString( SqliteHelpers.DecimalFormatNegative, CultureInfo.InvariantCulture );
    }
}
