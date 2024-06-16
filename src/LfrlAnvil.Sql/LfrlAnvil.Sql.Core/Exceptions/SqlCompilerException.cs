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

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred during preparation of an SQL expression.
/// </summary>
public class SqlCompilerException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlCompilerException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect.</param>
    /// <param name="error">Error message.</param>
    public SqlCompilerException(SqlDialect dialect, string error)
        : this( dialect, Chain.Create( error ) ) { }

    /// <summary>
    /// Creates a new <see cref="SqlCompilerException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect.</param>
    /// <param name="errors">Collection of error messages.</param>
    public SqlCompilerException(SqlDialect dialect, Chain<string> errors)
        : base( ExceptionResources.CompilerErrorsHaveOccurred( dialect, errors ) )
    {
        Dialect = dialect;
    }

    /// <summary>
    /// SQL dialect.
    /// </summary>
    public SqlDialect Dialect { get; }
}
