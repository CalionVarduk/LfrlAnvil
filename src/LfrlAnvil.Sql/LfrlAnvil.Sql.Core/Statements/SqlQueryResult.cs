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
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased query reader result.
/// </summary>
public readonly struct SqlQueryResult
{
    /// <summary>
    /// Represents a result without any <see cref="Rows"/>.
    /// </summary>
    public static readonly SqlQueryResult Empty = default;

    private readonly SqlResultSetField[]? _resultSetFields;

    /// <summary>
    /// Creates a new <see cref="SqlQueryResult"/> instance.
    /// </summary>
    /// <param name="resultSetFields">Collection of definitions of associated fields.</param>
    /// <param name="cells">1-dimensional collection of all field values for all rows.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="resultSetFields"/> is empty.</exception>
    /// <exception cref="ArgumentException">
    /// When number of <paramref name="cells"/> is not a multiple of the number of <paramref name="resultSetFields"/>.
    /// </exception>
    public SqlQueryResult(SqlResultSetField[] resultSetFields, List<object?> cells)
    {
        _resultSetFields = resultSetFields;
        Rows = cells.Count == 0 ? null : new SqlQueryReaderRowCollection( _resultSetFields, cells );
    }

    /// <summary>
    /// Collection of read rows.
    /// </summary>
    public SqlQueryReaderRowCollection? Rows { get; }

    /// <summary>
    /// Collection of definitions of associated fields.
    /// </summary>
    public ReadOnlySpan<SqlResultSetField> ResultSetFields => _resultSetFields;

    /// <summary>
    /// Specifies whether or not this result contains any <see cref="Rows"/>.
    /// </summary>
    [MemberNotNullWhen( false, nameof( Rows ) )]
    public bool IsEmpty => Rows is null;
}

/// <summary>
/// Represents a generic query reader result.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly struct SqlQueryResult<TRow>
    where TRow : notnull
{
    /// <summary>
    /// Represents a result without any <see cref="Rows"/>.
    /// </summary>
    public static readonly SqlQueryResult<TRow> Empty = default;

    private readonly SqlResultSetField[]? _resultSetFields;

    /// <summary>
    /// Creates a new <see cref="SqlQueryResult{TRow}"/> instance.
    /// </summary>
    /// <param name="resultSetFields">Collection of definitions of associated fields.</param>
    /// <param name="rows">Collection of read rows.</param>
    public SqlQueryResult(SqlResultSetField[]? resultSetFields, List<TRow> rows)
    {
        _resultSetFields = resultSetFields;
        Rows = rows.Count == 0 ? null : rows;
    }

    /// <summary>
    /// Collection of read rows.
    /// </summary>
    public List<TRow>? Rows { get; }

    /// <summary>
    /// Collection of definitions of associated fields.
    /// </summary>
    public ReadOnlySpan<SqlResultSetField> ResultSetFields => _resultSetFields;

    /// <summary>
    /// Specifies whether or not this result contains any <see cref="Rows"/>.
    /// </summary>
    [MemberNotNullWhen( false, nameof( Rows ) )]
    public bool IsEmpty => Rows is null;
}
