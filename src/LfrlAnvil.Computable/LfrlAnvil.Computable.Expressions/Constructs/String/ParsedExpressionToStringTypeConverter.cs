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
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

/// <summary>
/// Represents a type converter construct to <see cref="String"/> type.
/// </summary>
public sealed class ParsedExpressionToStringTypeConverter : ParsedExpressionTypeConverter<string>
{
    private readonly MethodInfo _toString;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionToStringTypeConverter"/> instance.
    /// </summary>
    public ParsedExpressionToStringTypeConverter()
    {
        _toString = MemberInfoLocator.FindToStringMethod();
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression TryCreateFromConstant(ConstantExpression operand)
    {
        Ensure.IsNotNull( operand.Value );
        return Expression.Constant( operand.Value.ToString() );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString );
    }
}
