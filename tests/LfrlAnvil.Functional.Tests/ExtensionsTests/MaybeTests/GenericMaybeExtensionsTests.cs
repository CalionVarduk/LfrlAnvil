using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.MaybeTests;

public abstract class GenericMaybeExtensionsTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void ToEither_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var sut = Maybe.Some( value );

        var result = sut.ToEither();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void ToEither_ShouldReturnCorrectResult_WhenDoesntHaveValue()
    {
        Maybe<T> sut = Maybe.None;
        var result = sut.ToEither();
        result.HasFirst.Should().BeFalse();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenDoesntHaveValue()
    {
        var sut = Maybe<Maybe<T>>.None;
        var result = sut.Reduce();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenHasUnderlyingValue()
    {
        var value = Fixture.Create<T>();
        var underlying = Maybe.Some( value );

        var sut = Maybe.Some( underlying );

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenDoesntHaveUnderlyingValue()
    {
        var underlying = Maybe<T>.None;

        var sut = Maybe.Some( underlying );

        var result = sut.Reduce();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void MatchWith_ShouldCallBothDelegate_WhenBothHaveValue()
    {
        var (value, otherValue, returnedValue) = Fixture.CreateDistinctCollection<T>( 3 );

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( _ => returnedValue );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

        var sut = Maybe.Some( value );
        var other = Maybe.Some( otherValue );

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            bothDelegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value, otherValue );
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void MatchWith_ShouldCallFirstDelegate_WhenOnlyFirstHasValue()
    {
        var (value, returnedValue) = Fixture.CreateDistinctCollection<T>( 2 );

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

        var sut = Maybe.Some( value );
        var other = Maybe<T>.None;

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            bothDelegate.Verify().CallCount.Should().Be( 0 );
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void MatchWith_ShouldCallSecondDelegate_WhenOnlySecondHasValue()
    {
        var (otherValue, returnedValue) = Fixture.CreateDistinctCollection<T>( 2 );

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => otherValue );

        var sut = Maybe<T>.None;
        var other = Maybe.Some( otherValue );

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            bothDelegate.Verify().CallCount.Should().Be( 0 );
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( otherValue );
            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void MatchWith_ShouldCallNoneDelegate_WhenBothDontHaveValue()
    {
        var returnedValue = Fixture.Create<T>();

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );

        var sut = Maybe<T>.None;
        var other = Maybe<T>.None;

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            bothDelegate.Verify().CallCount.Should().Be( 0 );
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            noneDelegate.Verify().CallCount.Should().Be( 1 );
        }
    }

    [Fact]
    public void MatchWith_WithAction_ShouldCallBothDelegate_WhenBothHaveValue()
    {
        var (value, otherValue) = Fixture.CreateDistinctCollection<T>( 2 );

        var bothDelegate = Substitute.For<Action<T, T>>();
        var firstDelegate = Substitute.For<Action<T>>();
        var secondDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe.Some( value );
        var other = Maybe.Some( otherValue );

        sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            bothDelegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( value, otherValue );
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void MatchWith_WithAction_ShouldCallFirstDelegate_WhenOnlyFirstHasValue()
    {
        var value = Fixture.Create<T>();

        var bothDelegate = Substitute.For<Action<T, T>>();
        var firstDelegate = Substitute.For<Action<T>>();
        var secondDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe.Some( value );
        var other = Maybe<T>.None;

        sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            bothDelegate.Verify().CallCount.Should().Be( 0 );
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void MatchWith_WithAction_ShouldCallSecondDelegate_WhenOnlySecondHasValue()
    {
        var otherValue = Fixture.Create<T>();

        var bothDelegate = Substitute.For<Action<T, T>>();
        var firstDelegate = Substitute.For<Action<T>>();
        var secondDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe<T>.None;
        var other = Maybe.Some( otherValue );

        sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            bothDelegate.Verify().CallCount.Should().Be( 0 );
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( otherValue );
            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void MatchWith_WithAction_ShouldCallNoneDelegate_WhenBothDontHaveValue()
    {
        var bothDelegate = Substitute.For<Action<T, T>>();
        var firstDelegate = Substitute.For<Action<T>>();
        var secondDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe<T>.None;
        var other = Maybe<T>.None;

        sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        using ( new AssertionScope() )
        {
            bothDelegate.Verify().CallCount.Should().Be( 0 );
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            noneDelegate.Verify().CallCount.Should().Be( 1 );
        }
    }
}
