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
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a constant construct.
/// </summary>
public class ParsedExpressionConstant
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstant"/> instance.
    /// </summary>
    /// <param name="type">Value's type.</param>
    /// <param name="value">Underlying value.</param>
    public ParsedExpressionConstant(Type type, object? value)
    {
        Expression = System.Linq.Expressions.Expression.Constant( value, type );
    }

    /// <summary>
    /// Underlying <see cref="System.Linq.Expressions.Expression"/>.
    /// </summary>
    public ConstantExpression Expression { get; }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstant{T}"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="T">value's type.</typeparam>
    /// <returns>New <see cref="ParsedExpressionConstant{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionConstant<T> Create<T>(T value)
    {
        return new ParsedExpressionConstant<T>( value );
    }
}

/// <summary>
/// Represents a constant construct.
/// </summary>
/// <typeparam name="T">Value's type.</typeparam>
public class ParsedExpressionConstant<T> : ParsedExpressionConstant
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionConstant{T}"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public ParsedExpressionConstant(T value)
        : base( typeof( T ), value ) { }
}
