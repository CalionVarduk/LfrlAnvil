using System.Collections.Generic;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class VariableAssignment
{
    internal VariableAssignment(
        BinaryExpression expression,
        IReadOnlyList<VariableAssignment> usedVariables,
        IReadOnlyList<InlineDelegateCollectionState.Result> delegates)
    {
        Expression = expression;
        UsedVariables = usedVariables;
        Delegates = delegates;
        IsUsed = false;
    }

    internal BinaryExpression Expression { get; }
    internal IReadOnlyList<VariableAssignment> UsedVariables { get; }
    internal IReadOnlyList<InlineDelegateCollectionState.Result> Delegates { get; }
    internal bool IsUsed { get; private set; }
    internal ParameterExpression Variable => ReinterpretCast.To<ParameterExpression>( Expression.Left );

    internal void MarkAsUsed()
    {
        if ( IsUsed )
            return;

        IsUsed = true;
        foreach ( var assignment in UsedVariables )
            assignment.MarkAsUsed();
    }
}
