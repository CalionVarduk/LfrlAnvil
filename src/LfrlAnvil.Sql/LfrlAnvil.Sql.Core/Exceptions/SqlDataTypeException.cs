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
/// Represents an error that occurred due to invalid <see cref="ISqlDataType"/> parameter values.
/// </summary>
public class SqlDataTypeException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="SqlDataTypeException"/> instance.
    /// </summary>
    /// <param name="parameters">Collection of (parameter-definition, invalid-value) pairs.</param>
    public SqlDataTypeException(Chain<Pair<SqlDataTypeParameter, int>> parameters)
        : base( ExceptionResources.InvalidDataTypeParameters( parameters ) ) { }
}
