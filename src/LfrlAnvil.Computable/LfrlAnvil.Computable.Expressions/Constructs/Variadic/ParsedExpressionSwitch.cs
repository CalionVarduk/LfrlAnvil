using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionSwitch : ParsedExpressionVariadicFunction
{
    private readonly ConstructorInfo _exceptionCtor;
    private readonly ConstantExpression _defaultBodyThrowFormat;

    public ParsedExpressionSwitch()
    {
        _exceptionCtor = MemberInfoLocator.FindInvocationExceptionCtor();
        _defaultBodyThrowFormat = Expression.Constant( Resources.SwitchValueWasNotHandledByAnyCaseFormat );
    }

    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, count: 2, nameof( parameters ) );

        var switchValue = parameters[0];
        var (switchCases, defaultBody) = ExtractSwitchCases( parameters );

        var result = switchValue.NodeType == ExpressionType.Constant
            ? CreateFromConstantValue( ReinterpretCast.To<ConstantExpression>( switchValue ), switchCases, defaultBody )
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

                var constantTest = ReinterpretCast.To<ConstantExpression>( test );
                if ( Equals( switchValue.Value, constantTest.Value ) )
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
    private (SwitchCase[] Cases, Expression DefaultBody) ExtractSwitchCases(IReadOnlyList<Expression> parameters)
    {
        var casesEnd = parameters.Count - 1;
        var defaultBody = TryGetSwitchCase( parameters[casesEnd] ) is null ? parameters[casesEnd] : null;
        if ( defaultBody is null )
        {
            defaultBody = CreateDefaultThrowBody( parameters[0] );
            ++casesEnd;
        }

        var cases = casesEnd == 1 ? Array.Empty<SwitchCase>() : new SwitchCase[casesEnd - 1];

        for ( var i = 1; i < casesEnd; ++i )
        {
            var switchCase = TryGetSwitchCase( parameters[i] );
            cases[i - 1] = switchCase ?? throw new ArgumentException( Resources.InvalidSwitchCaseParameter( i ), nameof( parameters ) );
        }

        var expectedType = GetExpectedType( cases, defaultBody );
        if ( expectedType is null )
            throw new ArgumentException( Resources.CannotDetermineSwitchReturnType, nameof( parameters ) );

        for ( var i = 0; i < cases.Length; ++i )
        {
            var expectedCaseBody = ExpressionHelpers.TryUpdateThrowType( cases[i].Body, expectedType );
            if ( ReferenceEquals( expectedCaseBody, cases[i].Body ) )
                continue;

            cases[i] = Expression.SwitchCase( expectedCaseBody, cases[i].TestValues );
        }

        defaultBody = ExpressionHelpers.TryUpdateThrowType( defaultBody, expectedType );

        return (cases, defaultBody);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private UnaryExpression CreateDefaultThrowBody(Expression switchValue)
    {
        var args = Expression.NewArrayInit(
            typeof( object ),
            switchValue.Type == typeof( object ) ? switchValue : Expression.Convert( switchValue, typeof( object ) ) );

        var exception = Expression.New( _exceptionCtor, _defaultBodyThrowFormat, args );
        return Expression.Throw( exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SwitchCase? TryGetSwitchCase(Expression expression)
    {
        return (expression as ConstantExpression)?.Value as SwitchCase;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type? GetExpectedType(SwitchCase[] cases, Expression defaultBody)
    {
        foreach ( var @case in cases )
        {
            if ( @case.Body.NodeType != ExpressionType.Throw )
                return @case.Body.Type;
        }

        if ( defaultBody.NodeType != ExpressionType.Throw )
            return defaultBody.Type;

        return null;
    }
}
