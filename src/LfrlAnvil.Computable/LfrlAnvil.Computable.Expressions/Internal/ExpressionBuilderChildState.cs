using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ExpressionBuilderChildState : ExpressionBuilderState
{
    private ExpressionBuilderChildState(
        ExpressionBuilderState parentState,
        Expectation expectation,
        int parenthesesCount,
        bool useDelegateParameters = false)
        : base( parentState, expectation, parenthesesCount, useDelegateParameters )
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
    internal static ExpressionBuilderChildState CreateArrayElements(ExpressionBuilderState parentState)
    {
        return new ExpressionBuilderChildState(
            parentState,
            Expectation.ArrayElementsStart,
            parenthesesCount: 0 );
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
            Expectation.ParameterType | Expectation.InlineDelegateParametersResolution,
            parenthesesCount: 0,
            useDelegateParameters: true );
    }
}
