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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct UnsafeBuilderResult<T>
{
    private UnsafeBuilderResult(T? result, Chain<ParsedExpressionBuilderError> errors)
    {
        Result = result;
        Errors = errors;
    }

    internal T? Result { get; }
    internal Chain<ParsedExpressionBuilderError> Errors { get; }

    [MemberNotNullWhen( true, nameof( Result ) )]
    internal bool IsOk => Errors.Count == 0;

    [Pure]
    internal UnsafeBuilderResult<TOther> CastErrorsTo<TOther>()
    {
        Assume.False( IsOk );
        return UnsafeBuilderResult<TOther>.CreateErrors( Errors );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateOk(T result)
    {
        return new UnsafeBuilderResult<T>( result, Chain<ParsedExpressionBuilderError>.Empty );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateErrors(ParsedExpressionBuilderError error)
    {
        return CreateErrors( Chain.Create( error ) );
    }

    [Pure]
    internal static UnsafeBuilderResult<T> CreateErrors(Chain<ParsedExpressionBuilderError> errors)
    {
        Assume.IsNotEmpty( errors );
        return new UnsafeBuilderResult<T>( default, errors );
    }
}
