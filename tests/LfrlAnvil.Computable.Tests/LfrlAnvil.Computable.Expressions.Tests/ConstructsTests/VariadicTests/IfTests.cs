using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class IfTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    public void Process_ShouldThrowArgumentException_WhenParameterCountIsNotEqualToThree(int count)
    {
        var parameters = Enumerable.Range( 0, count ).Select( _ => Expression.Constant( true ) ).ToArray();
        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenFirstParameterIsConstantAndNotOfBooleanType()
    {
        var parameters = new Expression[] { Expression.Constant( "foo" ), Expression.Constant( 1 ), Expression.Constant( 2 ) };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenFirstParameterIsNotConstantAndNotOfBooleanType()
    {
        var parameters = new Expression[] { Expression.Parameter( typeof( string ) ), Expression.Constant( 1 ), Expression.Constant( 2 ) };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldReturnSecondExpression_WhenFirstParameterIsConstantAndEqualToTrue()
    {
        var parameters = new Expression[] { Expression.Constant( true ), Expression.Constant( 1 ), Expression.Constant( "foo" ) };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        result.TestRefEquals( parameters[1] ).Go();
    }

    [Fact]
    public void Process_ShouldReturnThirdExpression_WhenFirstParameterIsConstantAndEqualToFalse()
    {
        var parameters = new Expression[] { Expression.Constant( false ), Expression.Constant( 1 ), Expression.Constant( "foo" ) };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        result.TestRefEquals( parameters[2] ).Go();
    }

    [Fact]
    public void Process_ShouldReturnConditionalExpression_WhenFirstParameterIsNotConstant()
    {
        var parameters = new Expression[] { Expression.Parameter( typeof( bool ) ), Expression.Constant( 1 ), Expression.Constant( 2 ) };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Conditional ),
                result.TestType()
                    .AssignableTo<ConditionalExpression>(
                        conditional => Assertion.All(
                            "Conditional",
                            conditional.Test.TestRefEquals( parameters[0] ),
                            conditional.IfTrue.TestRefEquals( parameters[1] ),
                            conditional.IfFalse.TestRefEquals( parameters[2] ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenFirstParameterIsNotConstantAndSecondAndThirdParametersHaveDifferentType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( bool ) ), Expression.Constant( 1 ), Expression.Constant( "foo" )
        };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenSecondAndThirdParametersAreThrowExpressions()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( bool ) ), Expression.Throw( exception ), Expression.Throw( exception )
        };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldReturnConditionalExpression_WhenFirstParameterIsNotConstantAndSecondParameterIsThrowExpression()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( bool ) ), Expression.Throw( exception ), Expression.Constant( 2 )
        };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Conditional ),
                result.TestType()
                    .AssignableTo<ConditionalExpression>(
                        conditional => Assertion.All(
                            "conditional",
                            conditional.Test.TestRefEquals( parameters[0] ),
                            conditional.IfTrue.NodeType.TestEquals( ExpressionType.Throw ),
                            conditional.IfFalse.TestRefEquals( parameters[2] ),
                            conditional.IfTrue.TestType()
                                .AssignableTo<UnaryExpression>(
                                    @throw => Assertion.All(
                                        "@throw",
                                        @throw.Type.TestEquals( parameters[2].Type ),
                                        @throw.Operand.TestRefEquals( exception ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnConditionalExpression_WhenFirstParameterIsNotConstantAndThirdParameterIsThrowExpression()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( bool ) ), Expression.Constant( 1 ), Expression.Throw( exception )
        };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Conditional ),
                result.TestType()
                    .AssignableTo<ConditionalExpression>(
                        conditional => Assertion.All(
                            "conditional",
                            conditional.Test.TestRefEquals( parameters[0] ),
                            conditional.IfTrue.TestRefEquals( parameters[1] ),
                            conditional.IfFalse.NodeType.TestEquals( ExpressionType.Throw ),
                            conditional.IfFalse.TestType()
                                .AssignableTo<UnaryExpression>(
                                    @throw => Assertion.All(
                                        "@throw",
                                        @throw.Type.TestEquals( parameters[1].Type ),
                                        @throw.Operand.TestRefEquals( exception ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnSecondExpressionWithCorrectType_WhenFirstParameterIsConstantAndEqualToTrueAndSecondExpressionIsThrow()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[] { Expression.Constant( true ), Expression.Throw( exception ), Expression.Constant( "foo" ) };

        var sut = new ParsedExpressionIf();

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
    public void Process_ShouldReturnThirdExpressionWithCorrectType_WhenFirstParameterIsConstantAndEqualToFalseAndThirdExpressionIsThrow()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[] { Expression.Constant( false ), Expression.Constant( "foo" ), Expression.Throw( exception ) };

        var sut = new ParsedExpressionIf();

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
