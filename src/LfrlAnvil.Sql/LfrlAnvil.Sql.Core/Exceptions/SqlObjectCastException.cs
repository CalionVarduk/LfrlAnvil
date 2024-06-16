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
/// Represents an error that occurred due to an SQL object of invalid type.
/// </summary>
public class SqlObjectCastException : InvalidCastException
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectCastException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect with which the object is associated.</param>
    /// <param name="expected">Expected object type.</param>
    /// <param name="actual">Actual object type.</param>
    public SqlObjectCastException(SqlDialect dialect, Type expected, Type actual)
        : base( ExceptionResources.GetObjectCastMessage( dialect, expected, actual ) )
    {
        Dialect = dialect;
        Expected = expected;
        Actual = actual;
    }

    /// <summary>
    /// SQL dialect with which the object is associated.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Expected object type.
    /// </summary>
    public Type Expected { get; }

    /// <summary>
    /// Actual object type.
    /// </summary>
    public Type Actual { get; }
}
