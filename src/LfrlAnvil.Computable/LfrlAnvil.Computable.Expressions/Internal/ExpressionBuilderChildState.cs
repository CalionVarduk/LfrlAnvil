using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ExpressionBuilderChildState : ExpressionBuilderState
{
    internal ExpressionBuilderChildState(ExpressionBuilderState parentState)
        : base( parentState, Expectation.OpenedParenthesis | Expectation.FunctionParametersStart )
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
}
