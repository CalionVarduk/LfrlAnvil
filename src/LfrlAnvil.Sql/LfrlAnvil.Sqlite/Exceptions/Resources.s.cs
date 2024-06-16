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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sqlite.Exceptions;

internal static class Resources
{
    internal const string ConnectionStringForPermanentDatabaseIsImmutable =
        "Connection string for permanent SQLite database is immutable.";

    internal const string ConnectionForClosedPermanentDatabaseCannotBeReopened =
        "Connection for closed permanent SQLite database cannot be reopened.";

    internal const string ConnectionStringToInMemoryDatabaseCannotBeModified =
        "Connection string to in-memory SQLite database cannot be modified.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ForeignKeyCheckFailure(Version version, IReadOnlySet<string> failedTableNames)
    {
        var headerText = $"Foreign key check for version {version} failed for {failedTableNames.Count} table(s):";
        var tablesText = string.Join( Environment.NewLine, failedTableNames.Select( (n, i) => $"{i + 1}. \"{n}\"" ) );
        return $"{headerText}{Environment.NewLine}{tablesText}";
    }
}
