using LfrlAnvil.Functional.Extensions;
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

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ToEither_ShouldReturnCorrectResult_WhenDoesntHaveValue()
    {
        Maybe<T> sut = Maybe.None;
        var result = sut.ToEither();
        result.HasFirst.TestFalse().Go();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenDoesntHaveValue()
    {
        var sut = Maybe<Maybe<T>>.None;
        var result = sut.Reduce();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenHasUnderlyingValue()
    {
        var value = Fixture.Create<T>();
        var underlying = Maybe.Some( value );

        var sut = Maybe.Some( underlying );

        var result = sut.Reduce();

        Assertion.All(
                result.HasValue.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Reduce_ShouldReturnCorrectResult_WhenDoesntHaveUnderlyingValue()
    {
        var underlying = Maybe<T>.None;

        var sut = Maybe.Some( underlying );

        var result = sut.Reduce();

        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void MatchWith_ShouldCallBothDelegate_WhenBothHaveValue()
    {
        var (value, otherValue, returnedValue) = Fixture.CreateManyDistinct<T>( count: 3 );

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( _ => returnedValue );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

        var sut = Maybe.Some( value );
        var other = Maybe.Some( otherValue );

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                bothDelegate.CallAt( 0 ).Exists.TestTrue(),
                bothDelegate.CallAt( 0 ).Arguments.TestSequence( [ value, otherValue ] ),
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void MatchWith_ShouldCallFirstDelegate_WhenOnlyFirstHasValue()
    {
        var (value, returnedValue) = Fixture.CreateManyDistinct<T>( count: 2 );

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

        var sut = Maybe.Some( value );
        var other = Maybe<T>.None;

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                bothDelegate.CallCount().TestEquals( 0 ),
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                secondDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void MatchWith_ShouldCallSecondDelegate_WhenOnlySecondHasValue()
    {
        var (otherValue, returnedValue) = Fixture.CreateManyDistinct<T>( count: 2 );

        var bothDelegate = Substitute.For<Func<T, T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var firstDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var secondDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => otherValue );

        var sut = Maybe<T>.None;
        var other = Maybe.Some( otherValue );

        var result = sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                bothDelegate.CallCount().TestEquals( 0 ),
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.TestSequence( [ otherValue ] ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( returnedValue ),
                bothDelegate.CallCount().TestEquals( 0 ),
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void MatchWith_WithAction_ShouldCallBothDelegate_WhenBothHaveValue()
    {
        var (value, otherValue) = Fixture.CreateManyDistinct<T>( count: 2 );

        var bothDelegate = Substitute.For<Action<T, T>>();
        var firstDelegate = Substitute.For<Action<T>>();
        var secondDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe.Some( value );
        var other = Maybe.Some( otherValue );

        sut.MatchWith( other, bothDelegate, firstDelegate, secondDelegate, noneDelegate );

        Assertion.All(
                bothDelegate.CallAt( 0 ).Exists.TestTrue(),
                bothDelegate.CallAt( 0 ).Arguments.TestSequence( [ value, otherValue ] ),
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                bothDelegate.CallCount().TestEquals( 0 ),
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                secondDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                bothDelegate.CallCount().TestEquals( 0 ),
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.TestSequence( [ otherValue ] ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                bothDelegate.CallCount().TestEquals( 0 ),
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 1 ) )
            .Go();
    }
}
