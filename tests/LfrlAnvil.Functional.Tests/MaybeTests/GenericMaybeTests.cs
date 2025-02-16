using System.Collections.Generic;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.MaybeTests;

[GenericTestClass( typeof( GenericMaybeTestsData<> ) )]
public abstract class GenericMaybeTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void None_ShouldNotHaveValue()
    {
        var sut = Maybe<T>.None;

        Assertion.All(
                sut.HasValue.TestFalse(),
                sut.Value.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void StaticNone_ShouldNotHaveValue()
    {
        var sut = ( Maybe<T> )Maybe.None;

        Assertion.All(
                sut.HasValue.TestFalse(),
                sut.Value.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Some_ShouldCreateWithValueWhenParameterIsNotNull()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = Maybe.Some( value );

        Assertion.All(
                sut.HasValue.TestTrue(),
                sut.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = Maybe.Some( value );

        var result = sut.GetHashCode();

        result.TestEquals( value.GetHashCode() ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnZero_WhenDoesntHaveValue()
    {
        var sut = Maybe<T>.None;

        var result = sut.GetHashCode();

        result.TestEquals( 0 ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
    {
        var a = hasValue1 ? Maybe.Some( value1 ) : Maybe<T>.None;
        var b = hasValue2 ? Maybe.Some( value2 ) : Maybe<T>.None;

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetValue_ShouldReturnUnderlyingValue_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = Maybe.Some( value );

        var result = sut.GetValue();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetValue_ShouldThrowValueAccessException_WhenDoesntHaveValue()
    {
        var sut = Maybe<T>.None;
        var action = Lambda.Of( () => sut.GetValue() );
        action.Test( exc => exc.TestType().Exact<ValueAccessException>( e => e.MemberName.TestEquals( nameof( Maybe<T>.Value ) ) ) ).Go();
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnUnderlyingValue_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = Maybe.Some( value );

        var result = sut.GetValueOrDefault();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDefaultValue_WhenDoesntHaveValue()
    {
        var sut = Maybe<T>.None;
        var result = sut.GetValueOrDefault();
        result.TestEquals( default ).Go();
    }

    [Fact]
    public void GetValueOrDefault_WithValue_ShouldReturnUnderlyingValue_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = Maybe.Some( value );

        var result = sut.GetValueOrDefault( Fixture.CreateNotDefault<T>() );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetValueOrDefault_WithValue_ShouldReturnDefaultValue_WhenDoesntHaveValue()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var sut = Maybe<T>.None;

        var result = sut.GetValueOrDefault( defaultValue );

        result.TestEquals( defaultValue ).Go();
    }

    [Fact]
    public void Bind_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( _ => Maybe.Some( returnedValue ) );

        var sut = Maybe.Some( value );

        var result = sut.Bind( some: someDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_ShouldNotCallSomeDelegateAndReturnNone_WhenDoesntHaveValue()
    {
        var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( i => Maybe.Some( i.ArgAt<T>( 0 ) ) );

        var sut = Maybe<T>.None;

        var result = sut.Bind( some: someDelegate );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void Bind_WithNone_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( _ => Maybe.Some( returnedValue ) );
        var noneDelegate = Substitute.For<Func<Maybe<T>>>().WithAnyArgs( _ => Maybe<T>.None );

        var sut = Maybe.Some( value );

        var result = sut.Bind( some: someDelegate, none: noneDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                noneDelegate.CallCount().TestEquals( 0 ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_WithNone_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( i => Maybe.Some( i.ArgAt<T>( 0 ) ) );
        var noneDelegate = Substitute.For<Func<Maybe<T>>>().WithAnyArgs( _ => Maybe.Some( returnedValue ) );

        var sut = Maybe<T>.None;

        var result = sut.Bind( some: someDelegate, none: noneDelegate );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 1 ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

        var sut = Maybe.Some( value );

        var result = sut.Match( some: someDelegate, none: noneDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                noneDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );

        var sut = Maybe<T>.None;

        var result = sut.Match( some: someDelegate, none: noneDelegate );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 1 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe.Some( value );

        sut.Match( some: someDelegate, none: noneDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                noneDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var someDelegate = Substitute.For<Action<T>>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe<T>.None;

        sut.Match( some: someDelegate, none: noneDelegate );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                noneDelegate.CallCount().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void IfSome_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = Maybe.Some( value );

        var result = sut.IfSome( someDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfSome_ShouldReturnNone_WhenDoesntHaveValue()
    {
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = Maybe<T>.None;

        var result = sut.IfSome( someDelegate );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfSome_WithAction_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Action<T>>();

        var sut = Maybe.Some( value );

        sut.IfSome( someDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void IfSome_WithAction_ShouldDoNothing_WhenDoesntHaveValue()
    {
        var someDelegate = Substitute.For<Action<T>>();

        var sut = Maybe<T>.None;

        sut.IfSome( someDelegate );

        someDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfSomeOrDefault_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = Maybe.Some( value );

        var result = sut.IfSomeOrDefault( someDelegate );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfSomeOrDefault_ShouldReturnDefault_WhenDoesntHaveValue()
    {
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = Maybe<T>.None;

        var result = sut.IfSomeOrDefault( someDelegate );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfSomeOrDefault_WithValue_ShouldCallSomeDelegate_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var returnedValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = Maybe.Some( value );

        var result = sut.IfSomeOrDefault( someDelegate, Fixture.CreateNotDefault<T>() );

        Assertion.All(
                someDelegate.CallAt( 0 ).Exists.TestTrue(),
                someDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfSomeOrDefault_WithValue_ShouldReturnDefault_WhenDoesntHaveValue()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = Maybe<T>.None;

        var result = sut.IfSomeOrDefault( someDelegate, defaultValue );

        Assertion.All(
                someDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void IfNone_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var returnedValue = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );

        var sut = Maybe<T>.None;

        var result = sut.IfNone( noneDelegate );

        Assertion.All(
                noneDelegate.CallCount().TestEquals( 1 ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfNone_ShouldReturnNone_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

        var sut = Maybe.Some( value );

        var result = sut.IfNone( noneDelegate );

        Assertion.All(
                noneDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfNone_WithAction_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe<T>.None;

        sut.IfNone( noneDelegate );

        noneDelegate.CallCount().TestEquals( 1 ).Go();
    }

    [Fact]
    public void IfNone_WithAction_ShouldDoNothing_WhenHasValueValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Action>();

        var sut = Maybe.Some( value );

        sut.IfNone( noneDelegate );

        noneDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfNoneOrDefault_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var returnedValue = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );
        var sut = Maybe<T>.None;

        var result = sut.IfNoneOrDefault( noneDelegate );

        Assertion.All(
                noneDelegate.CallCount().TestEquals( 1 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfNoneOrDefault_ShouldReturnDefault_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );
        var sut = Maybe.Some( value );

        var result = sut.IfNoneOrDefault( noneDelegate );

        Assertion.All(
                noneDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfNoneOrDefault_WithValue_ShouldCallNoneDelegate_WhenDoesntHaveValue()
    {
        var returnedValue = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );
        var sut = Maybe<T>.None;

        var result = sut.IfNoneOrDefault( noneDelegate, Fixture.CreateNotDefault<T>() );

        Assertion.All(
                noneDelegate.CallCount().TestEquals( 1 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfNoneOrDefault_WithValue_ShouldReturnDefault_WhenHasValue()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var value = Fixture.CreateNotDefault<T>();
        var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );
        var sut = Maybe.Some( value );

        var result = sut.IfNoneOrDefault( noneDelegate, defaultValue );

        Assertion.All(
                noneDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void MaybeConversionOperator_FromT_ShouldCreateWithValue_WhenParameterIsNotNull()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = ( Maybe<T> )value;

        Assertion.All(
                sut.HasValue.TestTrue(),
                sut.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void MaybeConversionOperator_FromNil_ReturnNone()
    {
        var sut = ( Maybe<T> )Nil.Instance;
        sut.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TConversionOperator_ShouldReturnUnderlyingValue_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = Maybe.Some( value );

        var result = ( T )sut;

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void TConversionOperator_ShouldThrowValueAccessException_WhenDoesntHaveValue()
    {
        var sut = Maybe<T>.None;
        var action = Lambda.Of( () => ( T )sut );
        action.Test( exc => exc.TestType().Exact<ValueAccessException>( e => e.MemberName.TestEquals( nameof( Maybe<T>.Value ) ) ) ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
    {
        var a = hasValue1 ? Maybe.Some( value1 ) : Maybe<T>.None;
        var b = hasValue2 ? Maybe.Some( value2 ) : Maybe<T>.None;

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
    {
        var a = hasValue1 ? Maybe.Some( value1 ) : Maybe<T>.None;
        var b = hasValue2 ? Maybe.Some( value2 ) : Maybe<T>.None;

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnOne_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = Maybe.Some( value );
        IReadOnlyCollection<T> collection = sut;

        var result = collection.Count;

        result.TestEquals( 1 ).Go();
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnZero_WhenDoesntHaveValue()
    {
        var sut = Maybe<T>.None;
        IReadOnlyCollection<T> collection = sut;

        var result = collection.Count;

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEnumeratorWithOneItem_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = Maybe.Some( value );
        sut.TestSequence( [ value ] ).Go();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEmptyEnumerator_WhenDoesntHaveValue()
    {
        var sut = Maybe<T>.None;
        sut.TestEmpty().Go();
    }
}
