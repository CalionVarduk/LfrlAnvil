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
using System.Linq.Expressions;

namespace LfrlAnvil.Sql.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid SQL compiler configuration.
/// </summary>
public class SqlCompilerConfigurationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlCompilerConfigurationException"/> instance.
    /// </summary>
    /// <param name="errors">Collection of underlying errors.</param>
    public SqlCompilerConfigurationException(Chain<Pair<Expression, Exception>> errors)
        : base( ExceptionResources.CompilerConfigurationErrorsHaveOccurred( errors ) )
    {
        Errors = errors;
    }

    /// <summary>
    /// Collection of underlying errors.
    /// </summary>
    public Chain<Pair<Expression, Exception>> Errors { get; }
}
