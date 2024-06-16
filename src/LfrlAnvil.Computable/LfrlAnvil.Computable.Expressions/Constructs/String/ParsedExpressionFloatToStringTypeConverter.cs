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
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

/// <summary>
/// Represents a <see cref="Single"/> to <see cref="String"/> type converter construct.
/// </summary>
public sealed class ParsedExpressionFloatToStringTypeConverter : ParsedExpressionTypeConverter<string, float>
{
    private readonly MethodInfo _toString;
    private readonly ConstantExpression _formatProvider;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFloatToStringTypeConverter"/> instance.
    /// </summary>
    /// <param name="formatProvider">Optional format provider. Equal to null by default.</param>
    public ParsedExpressionFloatToStringTypeConverter(IFormatProvider? formatProvider = null)
    {
        _formatProvider = Expression.Constant( formatProvider, typeof( IFormatProvider ) );
        _toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( typeof( float ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetSourceValue( operand, out var value )
            ? Expression.Constant( value.ToString( ReinterpretCast.To<IFormatProvider>( _formatProvider.Value ) ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString, _formatProvider );
    }
}
