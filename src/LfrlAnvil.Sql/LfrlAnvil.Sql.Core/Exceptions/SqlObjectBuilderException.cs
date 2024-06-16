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

using System;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid <see cref="ISqlObjectBuilder"/> state.
/// </summary>
public class SqlObjectBuilderException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderException"/> instance.
    /// </summary>
    /// <param name="dialect">SQL dialect with which the object is associated.</param>
    /// <param name="errors">Collection of error messages.</param>
    public SqlObjectBuilderException(SqlDialect dialect, Chain<string> errors)
        : base( ExceptionResources.GetObjectBuilderErrors( dialect, errors ) )
    {
        Dialect = dialect;
        Errors = errors;
    }

    /// <summary>
    /// SQL dialect with which the object is associated.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Collection of error messages.
    /// </summary>
    public Chain<string> Errors { get; }
}
