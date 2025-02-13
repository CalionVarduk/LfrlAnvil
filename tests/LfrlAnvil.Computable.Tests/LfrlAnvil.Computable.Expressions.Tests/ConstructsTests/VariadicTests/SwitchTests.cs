using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class SwitchTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void Process_ShouldThrowArgumentException_WhenParameterCountIsLessThanTwo(int count)
    {
        var parameters = Enumerable.Range( 0, count ).Select( _ => Expression.Constant( true ) ).ToArray();
        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenAnyParameterExpectedToBeSwitchCaseIsSomethingElse()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( "qux" ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenValueIsNotConstantAndAnyCaseValueHasDifferentType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2m ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenValueIsNotConstantAndNotAllCaseBodiesHaveTheSameType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( 0 ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsNotConstant()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Switch ),
                result.TestType()
                    .AssignableTo<SwitchExpression>(
                        @switch => Assertion.All(
                            "@switch",
                            @switch.SwitchValue.TestRefEquals( parameters[0] ),
                            @switch.DefaultBody.TestRefEquals( parameters[^1] ),
                            @switch.Cases.TestSequence(
                            [
                                ( SwitchCase )(( ConstantExpression )parameters[1]).Value!,
                                ( SwitchCase )(( ConstantExpression )parameters[2]).Value!,
                                ( SwitchCase )(( ConstantExpression )parameters[3]).Value!
                            ] ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnDefaultBody_WhenValueIsNotConstantAndThereAreNoSwitchCases()
    {
        var parameters = new Expression[] { Expression.Parameter( typeof( int ) ), Expression.Constant( "foobar" ) };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.TestRefEquals( parameters[^1] ).Go();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpressionWithImplicitDefaultBody_WhenValueIsNotConstantAndNoDefaultBodyIsProvided()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Switch ),
                result.TestType()
                    .AssignableTo<SwitchExpression>(
                        @switch => Assertion.All(
                            "@switch",
                            @switch.SwitchValue.TestRefEquals( parameters[0] ),
                            @switch.Cases.TestSequence( [ ( SwitchCase )(( ConstantExpression )parameters[1]).Value! ] ),
                            (@switch.DefaultBody?.NodeType).TestEquals( ExpressionType.Throw ),
                            @switch.DefaultBody.TestType()
                                .AssignableTo<UnaryExpression>(
                                    defaultThrow => Assertion.All(
                                        "defaultThrow",
                                        defaultThrow.Type.TestEquals( typeof( string ) ),
                                        defaultThrow.Operand.NodeType.TestEquals( ExpressionType.New ),
                                        defaultThrow.Operand.TestType()
                                            .AssignableTo<NewExpression>(
                                                exception => Assertion.All(
                                                    "exception",
                                                    exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                                    exception.Arguments.TestCount( count => count.TestEquals( 2 ) )
                                                        .Then(
                                                            args =>
                                                            {
                                                                var first = args[0];
                                                                var second = args[1];
                                                                return Assertion.All(
                                                                    "args",
                                                                    first.NodeType.TestEquals( ExpressionType.Constant ),
                                                                    second.NodeType.TestEquals( ExpressionType.NewArrayInit ),
                                                                    first.TestType()
                                                                        .AssignableTo<ConstantExpression>(
                                                                            constantArg =>
                                                                                constantArg.Value.TestEquals(
                                                                                    Resources.SwitchValueWasNotHandledByAnyCaseFormat ) ),
                                                                    second.TestType()
                                                                        .AssignableTo<NewArrayExpression>(
                                                                            arrayArg => Assertion.All(
                                                                                "arrayArg",
                                                                                arrayArg.Expressions.Count.TestEquals( 1 ),
                                                                                arrayArg.Expressions.TestAll(
                                                                                    (expr, _) => Assertion.All(
                                                                                        expr.NodeType.TestEquals( ExpressionType.Convert ),
                                                                                        expr.TestType()
                                                                                            .AssignableTo<UnaryExpression>(
                                                                                                argConvert =>
                                                                                                    argConvert.Operand.TestRefEquals(
                                                                                                        parameters[0] ) ) ) ) ) ) );
                                                            } ) ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnDefaultBody_WhenValueIsConstantAndAllCaseValuesAreConstantAndNoCaseValueEqualsValue()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 3 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.TestRefEquals( parameters[^1] ).Go();
    }

    [Fact]
    public void Process_ShouldReturnCorrectCaseBody_WhenValueIsConstantAndEqualsToOneOfConstantCaseValues()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.TestRefEquals( (( SwitchCase )(( ConstantExpression )parameters[3]).Value!).Body ).Go();
    }

    [Fact]
    public void Process_ShouldReturnCorrectCaseBody_WhenValueIsConstantNullAndEqualsToOneOfConstantCaseValues()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( null, typeof( string ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( "a" ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( "b" ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( null, typeof( string ) ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.TestRefEquals( (( SwitchCase )(( ConstantExpression )parameters[3]).Value!).Body ).Go();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsConstantAndAllCaseValuesAreVariable()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 0 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Switch ),
                result.TestType()
                    .AssignableTo<SwitchExpression>(
                        @switch => Assertion.All(
                            "@switch",
                            @switch.SwitchValue.TestRefEquals( parameters[0] ),
                            @switch.DefaultBody.TestRefEquals( parameters[^1] ),
                            @switch.Cases.TestSequence(
                            [
                                ( SwitchCase )(( ConstantExpression )parameters[1]).Value!,
                                ( SwitchCase )(( ConstantExpression )parameters[2]).Value!,
                                ( SwitchCase )(( ConstantExpression )parameters[3]).Value!
                            ] ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsConstantAndIsNotEqualToAnyConstantCaseValueAndSomeCaseValuesAreVariable()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Switch ),
                result.TestType()
                    .AssignableTo<SwitchExpression>(
                        @switch => Assertion.All(
                            "@switch",
                            @switch.SwitchValue.TestRefEquals( parameters[0] ),
                            @switch.DefaultBody.TestRefEquals( parameters[^1] ),
                            @switch.Cases.TestSequence( [ ( SwitchCase )(( ConstantExpression )parameters[3]).Value! ] ) ) ) )
            .Go();
    }

    [Fact]
    public void
        Process_ShouldReturnSwitchExpression_WhenValueIsConstantAndIsNotEqualToAnyConstantCaseValueAndSomeCaseValuesAreVariable_WhenCaseContainsBothConstantAndVariableValues()
    {
        var caseBody = Expression.Constant( "qux" );
        var caseParameter = Expression.Parameter( typeof( int ) );

        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( caseBody, Expression.Constant( 3 ), caseParameter ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Switch ),
                result.TestType()
                    .AssignableTo<SwitchExpression>(
                        @switch => Assertion.All(
                            "@switch",
                            @switch.SwitchValue.TestRefEquals( parameters[0] ),
                            @switch.DefaultBody.TestRefEquals( parameters[^1] ),
                            @switch.Cases.Count.TestEquals( 1 ),
                            @switch.Cases.TestAll(
                                (@case, _) => Assertion.All(
                                    "@case",
                                    @case.Body.TestRefEquals( caseBody ),
                                    @case.TestValues.TestSequence( [ caseParameter ] ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenAllCasesAreThrowExpressions()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Constant( 0 ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Throw( exception )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsNotConstantAndSomeCasesAreThrowExpressions()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Throw( exception )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Switch ),
                result.TestType()
                    .AssignableTo<SwitchExpression>(
                        @switch => Assertion.All(
                            "@switch",
                            @switch.SwitchValue.TestRefEquals( parameters[0] ),
                            (@switch.DefaultBody?.NodeType).TestEquals( ExpressionType.Throw ),
                            @switch.DefaultBody.TestType()
                                .AssignableTo<UnaryExpression>(
                                    defaultThrow => Assertion.All(
                                        "defaultThrow",
                                        defaultThrow.Type.TestEquals( typeof( string ) ),
                                        defaultThrow.Operand.TestRefEquals( exception ) ) ),
                            @switch.Cases.TestCount( count => count.TestEquals( 3 ) )
                                .Then(
                                    cases =>
                                    {
                                        var first = cases[0];
                                        var second = cases[1];
                                        var third = cases[2];
                                        return Assertion.All(
                                            "cases",
                                            first.TestRefEquals( ( SwitchCase )(( ConstantExpression )parameters[1]).Value! ),
                                            second.Body.NodeType.TestEquals( ExpressionType.Throw ),
                                            second.TestValues.TestSequence(
                                                (( SwitchCase )(( ConstantExpression )parameters[2]).Value!).TestValues ),
                                            third.TestRefEquals( ( SwitchCase )(( ConstantExpression )parameters[3]).Value! ),
                                            second.Body.TestType()
                                                .AssignableTo<UnaryExpression>(
                                                    caseThrow => Assertion.All(
                                                        "caseThrow",
                                                        caseThrow.Type.TestEquals( typeof( string ) ),
                                                        caseThrow.Operand.TestRefEquals( exception ) ) ) );
                                    } ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnCorrectCaseBody_WhenValueIsConstantAndEqualsToOneOfConstantCaseValuesWithThrowBody()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( string ) ),
                            @throw.Operand.TestRefEquals( exception ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnDefaultBody_WhenValueIsConstantAndDoesNorEqualToAnyConstantCaseValueAndDefaultBodyIsThrowExpression()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Constant( 3 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Throw( exception )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( string ) ),
                            @throw.Operand.TestRefEquals( exception ) ) ) )
            .Go();
    }
}
