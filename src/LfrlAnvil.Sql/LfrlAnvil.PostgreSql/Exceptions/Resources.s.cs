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
using System.Runtime.CompilerServices;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Exceptions;

internal static class Resources
{
    internal const string ConnectionStringMustIncludeDatabase = "PostgreSql connection string must include a database.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GeneratedColumnsWithVirtualStorageAreForbidden(PostgreSqlColumnBuilder column, SqlColumnComputation computation)
    {
        return
            $"Cannot set '{computation.Expression}' with virtual storage as computation of '{column}' because generated columns with virtual storage are forbidden.";
    }
}
