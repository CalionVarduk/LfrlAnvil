using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ConditionalValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenConditionIsPassedAndIfTrueValidatorReturnsEmptyChain()
    {
        var value = Fixture.Create<int>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => true );
        var ifTrue = Validators<string>.Pass<int>();
        var ifFalse = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var sut = Validators<string>.Conditional( condition, ifTrue, ifFalse );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeEmpty();
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenConditionIsPassedAndIfTrueValidatorReturnsChainWithFailure()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => true );
        var ifTrue = Validators<string>.Fail<int>( failure );
        var ifFalse = Validators<string>.Pass<int>();
        var sut = Validators<string>.Conditional( condition, ifTrue, ifFalse );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( failure );
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenConditionIsNotPassedAndIfFalseValidatorReturnsEmptyChain()
    {
        var value = Fixture.Create<int>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => false );
        var ifTrue = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var ifFalse = Validators<string>.Pass<int>();
        var sut = Validators<string>.Conditional( condition, ifTrue, ifFalse );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeEmpty();
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenConditionIsNotPassedAndIfFalseValidatorReturnsChainWithFailure()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => false );
        var ifTrue = Validators<string>.Pass<int>();
        var ifFalse = Validators<string>.Fail<int>( failure );
        var sut = Validators<string>.Conditional( condition, ifTrue, ifFalse );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( failure );
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_WhenCreatedWithIfTrue_ShouldReturnIfTrueResult_WhenConditionIsPassed()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => true );
        var ifTrue = Validators<string>.Fail<int>( failure );
        var sut = Validators<string>.IfTrue( condition, ifTrue );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( failure );
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_WhenCreatedWithIfTrue_ShouldReturnEmptyChain_WhenConditionIsNotPassed()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => false );
        var ifTrue = Validators<string>.Fail<int>( failure );
        var sut = Validators<string>.IfTrue( condition, ifTrue );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeEmpty();
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_WhenCreatedWithIfFalse_ShouldReturnIfFalseResult_WhenConditionIsNotPassed()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => false );
        var ifFalse = Validators<string>.Fail<int>( failure );
        var sut = Validators<string>.IfFalse( condition, ifFalse );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( failure );
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Validate_WhenCreatedWithIfFalse_ShouldReturnEmptyChain_WhenConditionIsPassed()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var condition = Substitute.For<Func<int, bool>>().WithAnyArgs( _ => true );
        var ifFalse = Validators<string>.Fail<int>( failure );
        var sut = Validators<string>.IfFalse( condition, ifFalse );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().BeEmpty();
            condition.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value );
        }
    }
}
