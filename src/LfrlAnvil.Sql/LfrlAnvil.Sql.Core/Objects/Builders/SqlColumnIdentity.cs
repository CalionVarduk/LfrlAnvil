// Copyright 2026 Łukasz Furlepa
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

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL column identity definition.
/// </summary>
public readonly record struct SqlColumnIdentity
{
    /// <summary>
    /// Represents a default SQL column identity definition.
    /// </summary>
    public static SqlColumnIdentity Default => new SqlColumnIdentity( null );

    /// <summary>
    /// Creates a new <see cref="SqlColumnIdentity"/> instance.
    /// </summary>
    /// <param name="autoIncrementCache">Optional number of cached values.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="autoIncrementCache"/> is not <b>null</b> and is less than <b>1</b>.
    /// </exception>
    public SqlColumnIdentity(int? autoIncrementCache = null)
    {
        if ( autoIncrementCache is not null )
            Ensure.IsGreaterThan( autoIncrementCache.Value, 0 );

        AutoIncrementCache = autoIncrementCache;
    }

    /// <summary>
    /// Number of cached values.
    /// </summary>
    public int? AutoIncrementCache { get; }
}
