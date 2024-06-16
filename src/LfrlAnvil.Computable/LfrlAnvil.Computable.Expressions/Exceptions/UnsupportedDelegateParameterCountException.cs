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

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to a number of nested delegate parameters with a closure being too large.
/// </summary>
public class UnsupportedDelegateParameterCountException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="UnsupportedDelegateParameterCountException"/> instance.
    /// </summary>
    /// <param name="parameterCount">Parameter count.</param>
    public UnsupportedDelegateParameterCountException(int parameterCount)
        : base( Resources.UnsupportedDelegateParameterCount( parameterCount ) )
    {
        ParameterCount = parameterCount;
    }

    /// <summary>
    /// Parameter count.
    /// </summary>
    public int ParameterCount { get; }
}
