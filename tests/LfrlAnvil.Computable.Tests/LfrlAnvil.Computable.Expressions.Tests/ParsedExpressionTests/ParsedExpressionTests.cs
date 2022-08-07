using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionTests;

public class ParsedExpressionTests : TestsBase
{
    [Fact]
    public void Expression_ShouldBeCreatedWithoutBoundArguments()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();

        var sut = factory.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            sut.Input.Should().Be( input );
            sut.GetArgumentCount().Should().Be( 2 );
            sut.GetUnboundArgumentCount().Should().Be( 2 );
            sut.GetBoundArgumentCount().Should().Be( 0 );
            sut.GetArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "b" );
            sut.GetUnboundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "b" );
            sut.GetBoundArgumentNames().Should().BeEmpty();
        }
    }

    [Fact]
    public void ToString_ShouldReturnInfoAboutGenericArgumentsAndInput()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "[double]", new ParsedExpressionTypeConverter<double>() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[double]", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, double>( input );

        var result = sut.ToString();

        result.Should().Be( "[System.Decimal => System.Double] a + 12.34 + b" );
    }

    [Theory]
    [InlineData( "a", true )]
    [InlineData( "b", true )]
    [InlineData( "c", false )]
    public void ContainsArgument_ShouldReturnTrueIfArgumentWithNameExists(string name, bool expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            sut.ContainsArgument( name ).Should().Be( expected );
            sut.ContainsArgument( name.AsMemory() ).Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "a", true )]
    [InlineData( "b", true )]
    [InlineData( "c", false )]
    public void ContainsUnboundArgument_ShouldReturnTrueIfUnboundArgumentWithNameExists(string name, bool expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            sut.ContainsUnboundArgument( name ).Should().Be( expected );
            sut.ContainsUnboundArgument( name.AsMemory() ).Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "a", 0 )]
    [InlineData( "b", 1 )]
    [InlineData( "c", -1 )]
    public void GetUnboundArgumentIndex_ShouldReturnCorrectResult(string name, int expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            sut.GetUnboundArgumentIndex( name ).Should().Be( expected );
            sut.GetUnboundArgumentIndex( name.AsMemory() ).Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( 0, "a" )]
    [InlineData( 1, "b" )]
    [InlineData( -1, "" )]
    [InlineData( 2, "" )]
    public void GetUnboundArgumentName_ShouldReturnCorrectResult(int index, string expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        sut.GetUnboundArgumentName( index ).ToString().Should().Be( expected );
    }

    [Fact]
    public void BindArguments_WithStringKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        using ( new AssertionScope() )
        {
            result.GetArgumentCount().Should().Be( 5 );
            result.GetUnboundArgumentCount().Should().Be( 2 );
            result.GetBoundArgumentCount().Should().Be( 3 );
            result.GetUnboundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "d" );
            result.GetBoundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "b", "c", "e" );
        }
    }

    [Fact]
    public void BindArguments_WithReadOnlyMemoryKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b".AsMemory(), bValue ),
            KeyValuePair.Create( "c".AsMemory(), cValue ),
            KeyValuePair.Create( "e".AsMemory(), eValue ) );

        using ( new AssertionScope() )
        {
            result.GetArgumentCount().Should().Be( 5 );
            result.GetUnboundArgumentCount().Should().Be( 2 );
            result.GetBoundArgumentCount().Should().Be( 3 );
            result.GetUnboundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "d" );
            result.GetBoundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "b", "c", "e" );
        }
    }

    [Fact]
    public void BindArguments_WithIndexKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( 1, bValue ),
            KeyValuePair.Create( 2, cValue ),
            KeyValuePair.Create( 4, eValue ) );

        using ( new AssertionScope() )
        {
            result.GetArgumentCount().Should().Be( 5 );
            result.GetUnboundArgumentCount().Should().Be( 2 );
            result.GetBoundArgumentCount().Should().Be( 3 );
            result.GetUnboundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "d" );
            result.GetBoundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "b", "c", "e" );
        }
    }

    [Fact]
    public void BindArguments_ShouldReturnThis_WhenParameterCollectionIsMaterializedAndEmpty()
    {
        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( Array.Empty<KeyValuePair<int, decimal>>() );

        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void BindArguments_ShouldReturnThis_WhenParameterCollectionIsNotMaterializedAndEmpty()
    {
        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            new[]
                {
                    KeyValuePair.Create( 1, 0m ),
                    KeyValuePair.Create( 2, 0m ),
                    KeyValuePair.Create( 4, 0m )
                }
                .Where( _ => false ) );

        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void BindArguments_CalledInChain_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut
            .BindArguments( KeyValuePair.Create( "b", bValue ) )
            .BindArguments( KeyValuePair.Create( "c", cValue ) )
            .BindArguments( KeyValuePair.Create( "e", eValue ) );

        using ( new AssertionScope() )
        {
            result.GetArgumentCount().Should().Be( 5 );
            result.GetUnboundArgumentCount().Should().Be( 2 );
            result.GetBoundArgumentCount().Should().Be( 3 );
            result.GetUnboundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "d" );
            result.GetBoundArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "b", "c", "e" );
        }
    }

    [Fact]
    public void
        BindArguments_ShouldThrowMathExpressionArgumentBindingException_WhenAnyArgumentToBindDoesNotExistInUnboundArgumentsCollection()
    {
        var (bValue, cValue, fValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var action = Lambda.Of(
            () => sut
                .BindArguments( KeyValuePair.Create( "b", bValue ) )
                .BindArguments( KeyValuePair.Create( "c", cValue ) )
                .BindArguments( KeyValuePair.Create( "f", fValue ) ) );

        action.Should().ThrowExactly<ParsedExpressionArgumentBindingException>();
    }

    [Fact]
    public void BindArguments_ShouldCreateExpressionThatCompilesToCorrectDelegate()
    {
        var (aValue, bValue, cValue, dValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 5 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        var unboundDelegate = sut.Compile();
        var boundDelegate = result.Compile();

        var unboundResult = unboundDelegate.Invoke( aValue, bValue, cValue, dValue, eValue );
        var boundResult = boundDelegate.Invoke( aValue, dValue );

        unboundResult.Should().Be( boundResult );
    }

    [Fact]
    public void BindArguments_ShouldNotModifyArrayIndexersOtherThanForTheParametersArray()
    {
        var (aValue, externalValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + externalValue;

        var input = "a + external_at 0";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "external_at", new ExternalArrayIndexUnaryOperator( externalValue ) )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "external_at", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke();

        resultValue.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void BindArguments_ShouldNotModifyParameterArrayIndexerWithIndexOutOfRange(int index)
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );

        var input = "a + external_parameter_accessor b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "external_parameter_accessor", new ParameterAccessorWithConstantIndexUnaryOperator( index ) )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "external_parameter_accessor", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", aValue ) );
        var @delegate = result.Compile();

        var action = Lambda.Of( () => @delegate.Invoke( bValue ) );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void BindArguments_ShouldNotModifyParameterArrayIndexerWithNonConstantIndex()
    {
        var (aValue, bValue, cValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );
        var expected = aValue + cValue + cValue;

        var input = "a + external_parameter_accessor b + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "external_parameter_accessor", new ParameterAccessorWithVariableIndexUnaryOperator( -1, 2 ) )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "external_parameter_accessor", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue, cValue );

        resultValue.Should().Be( expected );
    }

    [Theory]
    [InlineData( "b", true )]
    [InlineData( "c", true )]
    [InlineData( "e", true )]
    [InlineData( "a", false )]
    [InlineData( "d", false )]
    [InlineData( "f", false )]
    public void ContainsBoundArgument_ShouldReturnTrueIfBoundArgumentWithNameExists(string name, bool expected)
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        using ( new AssertionScope() )
        {
            result.ContainsBoundArgument( name ).Should().Be( expected );
            result.ContainsBoundArgument( name.AsMemory() ).Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "b", true, 12 )]
    [InlineData( "c", true, 34 )]
    [InlineData( "e", true, 56 )]
    [InlineData( "a", false, 0 )]
    [InlineData( "d", false, 0 )]
    [InlineData( "f", false, 0 )]
    public void TryGetBoundArgumentValue_ShouldReturnTrueAndBoundValueIfBoundArgumentWithNameExists(
        string name,
        bool expected,
        int expectedValue)
    {
        var (bValue, cValue, eValue) = (12m, 34m, 56m);

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        var exists = result.TryGetBoundArgumentValue( name, out var outResult );
        var existsAsMemory = result.TryGetBoundArgumentValue( name.AsMemory(), out var outAsMemoryResult );

        using ( new AssertionScope() )
        {
            exists.Should().Be( expected );
            existsAsMemory.Should().Be( expected );
            outResult.Should().Be( expectedValue );
            outAsMemoryResult.Should().Be( expectedValue );
        }
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableStringKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( "a", aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsStringKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableReadOnlyMemoryKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( "a".AsMemory(), aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsReadOnlyMemoryKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a".AsMemory(), aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableIndexKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( 0, aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsIndexKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( 0, aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    private sealed class ExternalArrayIndexUnaryOperator : ParsedExpressionUnaryOperator
    {
        private readonly ConstantExpression _array;

        internal ExternalArrayIndexUnaryOperator(params decimal[] values)
        {
            _array = Expression.Constant( values );
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            return Expression.ArrayIndex( _array, Expression.Convert( operand, typeof( int ) ) );
        }
    }

    private sealed class ParameterAccessorWithConstantIndexUnaryOperator : ParsedExpressionUnaryOperator
    {
        private readonly ConstantExpression _index;

        internal ParameterAccessorWithConstantIndexUnaryOperator(int index)
        {
            _index = Expression.Constant( index );
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            var parameterAccess = (BinaryExpression)operand;
            return Expression.ArrayIndex( parameterAccess.Left, _index );
        }
    }

    private sealed class ParameterAccessorWithVariableIndexUnaryOperator : ParsedExpressionUnaryOperator
    {
        private readonly BinaryExpression _index;

        internal ParameterAccessorWithVariableIndexUnaryOperator(int left, int right)
        {
            _index = Expression.Add( Expression.Constant( left ), Expression.Constant( right ) );
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            var parameterAccess = (BinaryExpression)operand;
            return Expression.ArrayIndex( parameterAccess.Left, _index );
        }
    }
}
