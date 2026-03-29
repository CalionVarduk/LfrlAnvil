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
using System.Diagnostics.Contracts;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Contains <see cref="SqlQueryResult{TRow}"/> extension methods for rows of ref type.
/// </summary>
public static class SqlQueryResultRefExtensions
{
    /// <summary>
    /// Returns the only row from <paramref name="source"/> query result.
    /// </summary>
    /// <param name="source">Query result.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>The only row or <b>null</b> if there are no rows.</returns>
    /// <exception cref="InvalidOperationException">When query result contains more than one row.</exception>
    [Pure]
    public static TRow? SingleOrDefault<TRow>(this SqlQueryResult<TRow> source)
        where TRow : class
    {
        if ( source.IsEmpty )
            return null;

        if ( source.Rows.Count > 1 )
            ExceptionThrower.Throw( new InvalidOperationException( Exceptions.ExceptionResources.QueryResultContainsMoreThanOneRow ) );

        return source.Rows[0];
    }
}
