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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ExpressionBuilderChildState : ExpressionBuilderState
{
    private ExpressionBuilderChildState(
        ExpressionBuilderState parentState,
        Expectation expectation,
        int parenthesesCount,
        bool isInlineDelegate = false)
        : base( parentState, expectation, parenthesesCount, isInlineDelegate )
    {
        ParentState = parentState;
        ElementCount = 0;
    }

    internal ExpressionBuilderState ParentState { get; }
    internal int ElementCount { get; private set; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void IncreaseElementCount()
    {
        ++ElementCount;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExpressionBuilderChildState CreateFunctionParameters(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.OpenedParenthesis | Expectation.FunctionParametersStart,
            parenthesesCount: -1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExpressionBuilderChildState CreateArrayElementsOrConstructorParameters(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.ArrayElementsStart | Expectation.OpenedParenthesis | Expectation.FunctionParametersStart,
            parenthesesCount: -1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExpressionBuilderChildState CreateInvocationParameters(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct,
            parenthesesCount: 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExpressionBuilderChildState CreateDelegate(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.ParameterType | Expectation.InlineParametersResolution,
            parenthesesCount: 0,
            isInlineDelegate: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExpressionBuilderChildState CreateVariable(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.VariableName,
            parenthesesCount: 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExpressionBuilderChildState CreateMacro(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.MacroName | Expectation.MacroParametersStart,
            parenthesesCount: 0 );
    }
}
