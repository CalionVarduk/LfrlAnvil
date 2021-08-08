using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Maybe
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
            var sut = (Maybe<T>) Core.Functional.Maybe.None;

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

            var sut = Core.Functional.Maybe.Some( value );

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

            var sut = Core.Functional.Maybe.Some( value );

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
            var a = hasValue1 ? Core.Functional.Maybe.Some( value1 ) : Maybe<T>.None;
            var b = hasValue2 ? Core.Functional.Maybe.Some( value2 ) : Maybe<T>.None;

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetValue_ShouldReturnUnderlyingValue_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.GetValue();

            result.Should().Be( value );
        }

        [Fact]
        public void GetValue_ShouldThrow_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;

            Action action = () =>
            {
                var _ = sut.GetValue();
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetValueOrDefault_ShouldReturnUnderlyingValue_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = Core.Functional.Maybe.Some( value );

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
            T? caughtValue = default;

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.Bind(
                some: v =>
                {
                    caughtValue = v;
                    return Core.Functional.Maybe.Some( returnedValue );
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Bind_ShouldNotCallSomeDelegateAndReturnNone_WhenDoesntHaveValue()
        {
            T? caughtValue = default;

            var sut = Maybe<T>.None;

            var result = sut.Bind(
                some: v =>
                {
                    caughtValue = v;
                    return Core.Functional.Maybe.Some( v );
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( default( T ) );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void Bind_WithNone_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;
            var wasNoneCalled = false;

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.Bind(
                some: v =>
                {
                    caughtValue = v;
                    return Core.Functional.Maybe.Some( returnedValue );
                },
                none: () =>
                {
                    wasNoneCalled = true;
                    return Maybe<T>.None;
                } );

            using ( new AssertionScope() )
            {
                wasNoneCalled.Should().BeFalse();
                caughtValue.Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Bind_WithNone_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;
            var wasNoneCalled = false;

            var sut = Maybe<T>.None;

            var result = sut.Bind(
                some: v =>
                {
                    caughtValue = v;
                    return Core.Functional.Maybe.Some( v );
                },
                none: () =>
                {
                    wasNoneCalled = true;
                    return Core.Functional.Maybe.Some( returnedValue );
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( default( T ) );
                wasNoneCalled.Should().BeTrue();
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;
            var wasNoneCalled = false;

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.Match(
                some: v =>
                {
                    caughtValue = v;
                    return returnedValue;
                },
                none: () =>
                {
                    wasNoneCalled = true;
                    return value;
                } );

            using ( new AssertionScope() )
            {
                wasNoneCalled.Should().BeFalse();
                caughtValue.Should().Be( value );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;
            var wasNoneCalled = false;

            var sut = Maybe<T>.None;

            var result = sut.Match(
                some: v =>
                {
                    caughtValue = v;
                    return v;
                },
                none: () =>
                {
                    wasNoneCalled = true;
                    return returnedValue;
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( default( T ) );
                wasNoneCalled.Should().BeTrue();
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;
            var wasNoneCalled = false;

            var sut = Core.Functional.Maybe.Some( value );

            sut.Match(
                some: v => { caughtValue = v; },
                none: () => { wasNoneCalled = true; } );

            using ( new AssertionScope() )
            {
                wasNoneCalled.Should().BeFalse();
                caughtValue.Should().Be( value );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            T? caughtValue = default;
            var wasNoneCalled = false;

            var sut = Maybe<T>.None;

            sut.Match(
                some: v => { caughtValue = v; },
                none: () => { wasNoneCalled = true; } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( default( T ) );
                wasNoneCalled.Should().BeTrue();
            }
        }

        [Fact]
        public void IfSome_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.IfSome(
                v =>
                {
                    caughtValue = v;
                    return returnedValue;
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfSome_ShouldReturnNone_WhenDoesntHaveValue()
        {
            T? caughtValue = default;

            var sut = Maybe<T>.None;

            var result = sut.IfSome(
                v =>
                {
                    caughtValue = v;
                    return v;
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( default( T ) );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfSome_WithAction_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;

            var sut = Core.Functional.Maybe.Some( value );

            sut.IfSome(
                v => { caughtValue = v; } );

            caughtValue.Should().Be( value );
        }

        [Fact]
        public void IfSome_WithAction_ShouldDoNothing_WhenDoesntHaveValue()
        {
            T? caughtValue = default;

            var sut = Maybe<T>.None;

            sut.IfSome(
                v => { caughtValue = v; } );

            caughtValue.Should().Be( default( T ) );
        }

        [Fact]
        public void IfSomeOrDefault_ShouldCallSomeDelegate_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var returnedValue = Fixture.CreateNotDefault<T>();
            T? caughtValue = default;

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.IfSomeOrDefault(
                v =>
                {
                    caughtValue = v;
                    return returnedValue;
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( value );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfSomeOrDefault_ShouldReturnDefault_WhenDoesntHaveValue()
        {
            T? caughtValue = default;

            var sut = Maybe<T>.None;

            var result = sut.IfSomeOrDefault(
                v =>
                {
                    caughtValue = v;
                    return v;
                } );

            using ( new AssertionScope() )
            {
                caughtValue.Should().Be( default( T ) );
                result.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void IfNone_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();

            var sut = Maybe<T>.None;

            var result = sut.IfNone(
                () => returnedValue );

            result.Value.Should().Be( returnedValue );
        }

        [Fact]
        public void IfNone_ShouldReturnNone_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var wasNoneCalled = false;

            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.IfNone(
                () =>
                {
                    wasNoneCalled = true;
                    return value;
                } );

            using ( new AssertionScope() )
            {
                wasNoneCalled.Should().BeFalse();
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfNone_WithAction_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var wasNoneCalled = false;

            var sut = Maybe<T>.None;

            sut.IfNone(
                () => { wasNoneCalled = true; } );

            wasNoneCalled.Should().BeTrue();
        }

        [Fact]
        public void IfNone_WithAction_ShouldDoNothing_WhenHasValueValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var wasNoneCalled = false;

            var sut = Core.Functional.Maybe.Some( value );

            sut.IfNone(
                () => { wasNoneCalled = true; } );

            wasNoneCalled.Should().BeFalse();
        }

        [Fact]
        public void IfNoneOrDefault_ShouldCallNoneDelegate_WhenDoesntHaveValue()
        {
            var returnedValue = Fixture.CreateNotDefault<T>();
            var sut = Maybe<T>.None;

            var result = sut.IfNoneOrDefault(
                () => returnedValue );

            result.Should().Be( returnedValue );
        }

        [Fact]
        public void IfNoneOrDefault_ShouldReturnDefault_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var wasNoneCalled = false;
            var sut = Core.Functional.Maybe.Some( value );

            var result = sut.IfNoneOrDefault(
                () =>
                {
                    wasNoneCalled = true;
                    return value;
                } );

            using ( new AssertionScope() )
            {
                wasNoneCalled.Should().BeFalse();
                result.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void MaybeConversionOperator_FromT_ShouldCreateWithValue_WhenParameterIsNotNull()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = (Maybe<T>) value;

            using ( new AssertionScope() )
            {
                sut.HasValue.Should().BeTrue();
                sut.Value.Should().Be( value );
            }
        }

        [Fact]
        public void MaybeConversionOperator_FromNil_ReturnNone()
        {
            var sut = (Maybe<T>) Core.Functional.Nil.Instance;

            sut.HasValue.Should().BeFalse();
        }

        [Fact]
        public void TConversionOperator_ShouldReturnUnderlyingValue_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = Core.Functional.Maybe.Some( value );

            var result = (T) sut;

            result.Should().Be( value );
        }

        [Fact]
        public void TConversionOperator_ShouldThrow_WhenDoesntHaveValue()
        {
            var sut = Maybe<T>.None;

            Action action = () =>
            {
                var _ = (T) sut;
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
        {
            var a = hasValue1 ? Core.Functional.Maybe.Some( value1 ) : Maybe<T>.None;
            var b = hasValue2 ? Core.Functional.Maybe.Some( value2 ) : Maybe<T>.None;

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericMaybeTestsData<T>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(T? value1, bool hasValue1, T? value2, bool hasValue2, bool expected)
        {
            var a = hasValue1 ? Core.Functional.Maybe.Some( value1 ) : Maybe<T>.None;
            var b = hasValue2 ? Core.Functional.Maybe.Some( value2 ) : Maybe<T>.None;

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
