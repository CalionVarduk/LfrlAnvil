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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a factory of SQL node interpreters.
/// </summary>
public interface ISqlNodeInterpreterFactory
{
    /// <summary>
    /// Creates a new <see cref="SqlNodeInterpreter"/> instance.
    /// </summary>
    /// <param name="context">Underlying context.</param>
    /// <returns>New <see cref="SqlNodeInterpreter"/> instance.</returns>
    [Pure]
    SqlNodeInterpreter Create(SqlNodeInterpreterContext context);
}
