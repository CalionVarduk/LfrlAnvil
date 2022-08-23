using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionSwitch : ParsedExpressionVariadicFunction
{
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, count: 2, nameof( parameters ) );

        var switchValue = parameters[0];
        var (switchCases, defaultBody) = ExtractSwitchCases( parameters );

        var result = switchValue.NodeType == ExpressionType.Constant
            ? CreateFromConstantValue( (ConstantExpression)switchValue, switchCases, defaultBody )
            : CreateFromVariableValue( switchValue, switchCases, defaultBody );

        return result;
    }

    [Pure]
    private static Expression CreateFromConstantValue(ConstantExpression switchValue, SwitchCase[] switchCases, Expression defaultBody)
    {
        var variableCaseCount = 0;

        foreach ( var @case in switchCases )
        {
            var isCountedAsVariable = false;

            foreach ( var test in @case.TestValues )
            {
                if ( test.NodeType != ExpressionType.Constant && ! isCountedAsVariable )
                {
                    isCountedAsVariable = true;
                    ++variableCaseCount;
                    continue;
                }

                var constantTest = (ConstantExpression)test;
                if ( switchValue.Value is null )
                {
                    if ( constantTest.Value is null )
                        return @case.Body;

                    continue;
                }

                if ( switchValue.Value.Equals( constantTest.Value ) )
                    return @case.Body;
            }
        }

        if ( variableCaseCount == 0 )
            return defaultBody;

        var caseIndex = 0;
        var cases = new SwitchCase[variableCaseCount];
        foreach ( var @case in switchCases )
        {
            if ( @case.TestValues.All( t => t.NodeType == ExpressionType.Constant ) )
                continue;

            cases[caseIndex++] = @case.TestValues.All( t => t.NodeType != ExpressionType.Constant )
                ? @case
                : Expression.SwitchCase( @case.Body, @case.TestValues.Where( t => t.NodeType != ExpressionType.Constant ) );
        }

        var result = Expression.Switch( switchValue, defaultBody, cases );
        return result;
    }

    [Pure]
    private static Expression CreateFromVariableValue(Expression switchValue, SwitchCase[] switchCases, Expression defaultBody)
    {
        if ( switchCases.Length == 0 )
            return defaultBody;

        var result = Expression.Switch( switchValue, defaultBody, switchCases );
        return result;
    }

    [Pure]
    private static (SwitchCase[] Cases, Expression DefaultBody) ExtractSwitchCases(IReadOnlyList<Expression> parameters)
    {
        var lastIndex = parameters.Count - 1;
        var cases = lastIndex == 1 ? Array.Empty<SwitchCase>() : new SwitchCase[lastIndex - 1];
        var defaultBody = parameters[lastIndex];

        for ( var i = 1; i < lastIndex; ++i )
        {
            var switchCase = TryGetSwitchCase( parameters[i] );
            cases[i - 1] = switchCase ?? throw new ArgumentException( Resources.InvalidSwitchCaseParameter( i ), nameof( parameters ) );
        }

        return (cases, defaultBody);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SwitchCase? TryGetSwitchCase(Expression expression)
    {
        return (expression as ConstantExpression)?.Value as SwitchCase;
    }
}
