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

namespace LfrlAnvil.Sqlite.Exceptions;

/// <summary>
/// Represents an error that occurred during foreign key constraint validation.
/// </summary>
/// <remarks>See <see cref="SqliteDatabaseFactoryOptions.AreForeignKeyChecksDisabled"/> for more information.</remarks>
public class SqliteForeignKeyCheckException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqliteForeignKeyCheckException"/> instance.
    /// </summary>
    /// <param name="version">Version's identifier.</param>
    /// <param name="failedTableNames">Collection of names of tables for which the foreign key constraint validation has failed.</param>
    public SqliteForeignKeyCheckException(Version version, IReadOnlySet<string> failedTableNames)
        : base( Resources.ForeignKeyCheckFailure( version, failedTableNames ) )
    {
        Version = version;
        FailedTableNames = failedTableNames;
    }

    /// <summary>
    /// Version's identifier.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Collection of names of tables for which the foreign key constraint validation has failed.
    /// </summary>
    public IReadOnlySet<string> FailedTableNames { get; }
}
