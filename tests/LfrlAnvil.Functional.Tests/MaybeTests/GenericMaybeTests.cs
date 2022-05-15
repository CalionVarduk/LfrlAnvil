using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Functional.Tests.MaybeTests
{
    [GenericTestClass( typeof( GenericMaybeTestsData<> ) )]
    public abstract class GenericMaybeTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void None_ShouldNotHaveValue()
        {
            var sut = Maybe<T>.None;

            using ( new AssertionScope() )
            {
                sut.HasValue.Should().BeFalse();
                sut.Value.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void StaticNone_ShouldNotHaveValue()
        {
            var sut = (Maybe<T>)Maybe.None;

            using ( new AssertionScope() )
            {
                sut.HasValue.Should().BeFalse();
                sut.Value.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void Some_ShouldCreateWithValueWhenParameterIsNotNull()
        {
            var value = Fixture.CreateNotDefault<T>();
            var sut = Maybe.Some( value );

            using ( new AssertionScope() )
            {
                sut.HasValue.Should().BeTrue();
                sut.Value.Should().Be( value );
            }
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var sut = Maybe.Some( value );

            var result = sut.GetHashCode();

            result.Should().Be( value.GetHashCode() );
        }

        [Fact]
        public void GetHashCode_ShouldReturnZero_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;

            var result = sut.GetHashCode();

            result.Should().Be( 0 );
        }

        [Theory]
        [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
        {
            var a = hasValue1 ? Maybe.Some( value1 ) : Maybe<T>.None;
            var b = hasValue2 ? Maybe.Some( value2 ) : Maybe<T>.None;

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetValue_ShouldReturnUnderlyingValue_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var sut = Maybe.Some( value );

            var result = sut.GetValue();

            result.Should().Be( value );
        }

        [Fact]
        public void GetValue_ShouldThrowValueAccessException_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;
            var action = Lambda.Of( () => sut.GetValue() );
            action.Should().ThrowExactly<ValueAccessException>().AndMatch( e => e.MemberName == nameof( Maybe<T>.Value ) );
        }

        [Fact]
        public void GetValueOrDefault_ShouldReturnUnderlyingValue_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var sut = Maybe.Some( value );

            var result = sut.GetValueOrDefault();

            result.Should().Be( value );
        }

        [Fact]
        public void GetValueOrDefault_ShouldReturnDefaultValue_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;
            var result = sut.GetValueOrDefault();
            result.Should().Be( default( T ) );
        }

        [Fact]
        public void Bind_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( _ => Maybe.Some( returnedValue ) );

            var sut = Maybe.Some( value );

            var result = sut.Bind( some: someDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Bind_ShouldNotCallSomeDelegateAndReturnNone_WhenDoesntHaveValue()
        {
            var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( i => Maybe.Some( i.ArgAt<T>( 0 ) ) );

            var sut = Maybe<T>.None;

            var result = sut.Bind( some: someDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void Bind_WithNone_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( _ => Maybe.Some( returnedValue ) );
            var noneDelegate = Substitute.For<Func<Maybe<T>>>().WithAnyArgs( _ => Maybe<T>.None );

            var sut = Maybe.Some( value );

            var result = sut.Bind(
                some: someDelegate,
                none: noneDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                noneDelegate.Verify().CallCount.Should().Be( 0 );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Bind_WithNone_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, Maybe<T>>>().WithAnyArgs( i => Maybe.Some( i.ArgAt<T>( 0 ) ) );
            var noneDelegate = Substitute.For<Func<Maybe<T>>>().WithAnyArgs( _ => Maybe.Some( returnedValue ) );

            var sut = Maybe<T>.None;

            var result = sut.Bind(
                some: someDelegate,
                none: noneDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallCount.Should().Be( 0 );
                noneDelegate.Verify().CallCount.Should().Be( 1 );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
            var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

            var sut = Maybe.Some( value );

            var result = sut.Match(
                some: someDelegate,
                none: noneDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                noneDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
            var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );

            var sut = Maybe<T>.None;

            var result = sut.Match(
                some: someDelegate,
                none: noneDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallCount.Should().Be( 0 );
                noneDelegate.Verify().CallCount.Should().Be( 1 );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Action<T>>();
            var noneDelegate = Substitute.For<Action>();

            var sut = Maybe.Some( value );

            sut.Match(
                some: someDelegate,
                none: noneDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                noneDelegate.Verify().CallCount.Should().Be( 0 );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var someDelegate = Substitute.For<Action<T>>();
            var noneDelegate = Substitute.For<Action>();

            var sut = Maybe<T>.None;

            sut.Match(
                some: someDelegate,
                none: noneDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallCount.Should().Be( 0 );
                noneDelegate.Verify().CallCount.Should().Be( 1 );
            }
        }

        [Fact]
        public void IfSome_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

            var sut = Maybe.Some( value );

            var result = sut.IfSome( someDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfSome_ShouldReturnNone_WhenDoesntHaveValue()
        {
            var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

            var sut = Maybe<T>.None;

            var result = sut.IfSome( someDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfSome_WithAction_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Action<T>>();

            var sut = Maybe.Some( value );

            sut.IfSome( someDelegate );

            someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }

        [Fact]
        public void IfSome_WithAction_ShouldDoNothing_WhenDoesntHaveValue()
        {
            var someDelegate = Substitute.For<Action<T>>();

            var sut = Maybe<T>.None;

            sut.IfSome( someDelegate );

            someDelegate.Verify().CallCount.Should().Be( 0 );
        }

        [Fact]
        public void IfSomeOrDefault_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

            var sut = Maybe.Some( value );

            var result = sut.IfSomeOrDefault( someDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfSomeOrDefault_ShouldReturnDefault_WhenDoesntHaveValue()
        {
            var someDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

            var sut = Maybe<T>.None;

            var result = sut.IfSomeOrDefault( someDelegate );

            using ( new AssertionScope() )
            {
                someDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void IfNone_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );

            var sut = Maybe<T>.None;

            var result = sut.IfNone( noneDelegate );

            using ( new AssertionScope() )
            {
                noneDelegate.Verify().CallCount.Should().Be( 1 );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfNone_ShouldReturnNone_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );

            var sut = Maybe.Some( value );

            var result = sut.IfNone( noneDelegate );

            using ( new AssertionScope() )
            {
                noneDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfNone_WithAction_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var noneDelegate = Substitute.For<Action>();

            var sut = Maybe<T>.None;

            sut.IfNone( noneDelegate );

            noneDelegate.Verify().CallCount.Should().Be( 1 );
        }

        [Fact]
        public void IfNone_WithAction_ShouldDoNothing_WhenHasValueValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var noneDelegate = Substitute.For<Action>();

            var sut = Maybe.Some( value );

            sut.IfNone( noneDelegate );

            noneDelegate.Verify().CallCount.Should().Be( 0 );
        }

        [Fact]
        public void IfNoneOrDefault_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => returnedValue );
            var sut = Maybe<T>.None;

            var result = sut.IfNoneOrDefault( noneDelegate );

            using ( new AssertionScope() )
            {
                noneDelegate.Verify().CallCount.Should().Be( 1 );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfNoneOrDefault_ShouldReturnDefault_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var noneDelegate = Substitute.For<Func<T>>().WithAnyArgs( _ => value );
            var sut = Maybe.Some( value );

            var result = sut.IfNoneOrDefault( noneDelegate );

            using ( new AssertionScope() )
            {
                noneDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void MaybeConversionOperator_FromT_ShouldCreateWithValue_WhenParameterIsNotNull()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = (Maybe<T>)value;

            using ( new AssertionScope() )
            {
                sut.HasValue.Should().BeTrue();
                sut.Value.Should().Be( value );
            }
        }

        [Fact]
        public void MaybeConversionOperator_FromNil_ReturnNone()
        {
            var sut = (Maybe<T>)Nil.Instance;
            sut.HasValue.Should().BeFalse();
        }

        [Fact]
        public void TConversionOperator_ShouldReturnUnderlyingValue_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = Maybe.Some( value );

            var result = (T)sut;

            result.Should().Be( value );
        }

        [Fact]
        public void TConversionOperator_ShouldThrowValueAccessException_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;
            var action = Lambda.Of( () => (T)sut );
            action.Should().ThrowExactly<ValueAccessException>().AndMatch( e => e.MemberName == nameof( Maybe<T>.Value ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
        {
            var a = hasValue1 ? Maybe.Some( value1 ) : Maybe<T>.None;
            var b = hasValue2 ? Maybe.Some( value2 ) : Maybe<T>.None;

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
        {
            var a = hasValue1 ? Maybe.Some( value1 ) : Maybe<T>.None;
            var b = hasValue2 ? Maybe.Some( value2 ) : Maybe<T>.None;

            var result = a != b;

            result.Should().Be( expected );
        }

        [Fact]
        public void IReadOnlyCollectionCount_ShouldReturnOne_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = Maybe.Some( value );
            IReadOnlyCollection<T> collection = sut;

            var result = collection.Count;

            result.Should().Be( 1 );
        }

        [Fact]
        public void IReadOnlyCollectionCount_ShouldReturnZero_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;
            IReadOnlyCollection<T> collection = sut;

            var result = collection.Count;

            result.Should().Be( 0 );
        }

        [Fact]
        public void IEnumerableGetEnumerator_ShouldReturnEnumeratorWithOneItem_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var sut = Maybe.Some( value );
            sut.Should().BeSequentiallyEqualTo( value );
        }

        [Fact]
        public void IEnumerableGetEnumerator_ShouldReturnEmptyEnumerator_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;
            sut.Should().BeEmpty();
        }
    }
}
