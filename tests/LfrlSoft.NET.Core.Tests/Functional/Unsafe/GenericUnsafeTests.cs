using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using LfrlSoft.NET.TestExtensions.FluentAssertions;
using LfrlSoft.NET.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Unsafe
{
    [GenericTestClass( typeof( GenericUnsafeTestsData<> ) )]
    public abstract class GenericUnsafeTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void Try_ShouldReturnCorrectResult_WhenDelegateDoesntThrow()
        {
            var value = Fixture.Create<T>();

            T Action()
            {
                return value;
            }

            var result = Core.Functional.Unsafe.Try( Action );

            using ( new AssertionScope() )
            {
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void Try_ShouldReturnCorrectResult_WhenDelegateThrows()
        {
            var error = new Exception();

            T Action()
            {
                throw error;
            }

            var result = Core.Functional.Unsafe.Try( Action );

            using ( new AssertionScope() )
            {
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
        }

        [Fact]
        public void Empty_ShouldHaveDefaultValue()
        {
            var sut = Unsafe<T>.Empty;

            using ( new AssertionScope() )
            {
                sut.IsOk.Should().BeTrue();
                sut.HasError.Should().BeFalse();
                sut.Value.Should().Be( default( T ) );
                sut.Error.Should().BeNull();
            }
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var sut = (Unsafe<T>)value;
            var expected = Core.Hash.Default.Add( value ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult_WhenHasError()
        {
            var error = new Exception();
            var sut = (Unsafe<T>)error;
            var expected = Core.Hash.Default.Add( error ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericUnsafeTestsData<T>.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(object value1, bool isOk1, object value2, bool isOk2, bool expected)
        {
            var a = (Unsafe<T>)(isOk1 ? (T)value1 : (Exception)value1);
            var b = (Unsafe<T>)(isOk2 ? (T)value2 : (Exception)value2);

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetValue_ShouldReturnCorrectResult_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            IUnsafe sut = (Unsafe<T>)value;

            var result = sut.GetValue();

            result.Should().Be( value );
        }

        [Fact]
        public void GetValue_ShouldThrowArgumentNullException_WhenHasError()
        {
            var error = new Exception();
            IUnsafe sut = (Unsafe<T>)error;

            var action = Core.Functional.Lambda.Of( () => sut.GetValue() );

            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void GetValueOrDefault_ShouldReturnCorrectResult_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            IUnsafe sut = (Unsafe<T>)value;

            var result = sut.GetValueOrDefault();

            result.Should().Be( value );
        }

        [Fact]
        public void GetValueOrDefault_ShouldReturnDefaultValue_WhenHasError()
        {
            var error = new Exception();
            IUnsafe sut = (Unsafe<T>)error;

            var result = sut.GetValueOrDefault();

            result.Should().Be( default( T ) );
        }

        [Fact]
        public void GetError_ShouldReturnCorrectResult_WhenHasError()
        {
            var error = new Exception();
            IUnsafe sut = (Unsafe<T>)error;

            var result = sut.GetError();

            result.Should().Be( error );
        }

        [Fact]
        public void GetError_ShouldThrowArgumentNullException_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            IUnsafe sut = (Unsafe<T>)value;

            var action = Core.Functional.Lambda.Of( () => sut.GetError() );

            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void GetErrorOrDefault_ShouldReturnCorrectResult_WhenHasError()
        {
            var error = new Exception();
            IUnsafe sut = (Unsafe<T>)error;

            var result = sut.GetErrorOrDefault();

            result.Should().Be( error );
        }

        [Fact]
        public void GetErrorOrDefault_ShouldReturnNull_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            IUnsafe sut = (Unsafe<T>)value;

            var result = sut.GetErrorOrDefault();

            result.Should().BeNull();
        }

        [Fact]
        public void Bind_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, Unsafe<T>>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)value;

            var result = sut.Bind( okDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Bind_ShouldNotCallOkDelegateAndReturnCorrectResult_WhenHasError()
        {
            var error = new Exception();
            var okDelegate = Substitute.For<Func<T, Unsafe<T>>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

            var sut = (Unsafe<T>)error;

            var result = sut.Bind( okDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
        }

        [Fact]
        public void Bind_WithError_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, Unsafe<T>>>().WithAnyArgs( _ => returnedValue );
            var errorDelegate = Substitute.For<Func<Exception, Unsafe<T>>>().WithAnyArgs( _ => value );

            var sut = (Unsafe<T>)value;

            var result = sut.Bind( okDelegate, errorDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                errorDelegate.Verify().CallCount.Should().Be( 0 );
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Bind_WithError_ShouldCallErrorDelegate_WhenHasError()
        {
            var error = new Exception();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, Unsafe<T>>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
            var errorDelegate = Substitute.For<Func<Exception, Unsafe<T>>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)error;

            var result = sut.Bind( okDelegate, errorDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallCount.Should().Be( 0 );
                errorDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( error );
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
            var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

            var sut = (Unsafe<T>)value;

            var result = sut.Match( okDelegate, errorDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                errorDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_ShouldCallErrorDelegate_WhenHasError()
        {
            var error = new Exception();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
            var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)error;

            var result = sut.Match( okDelegate, errorDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallCount.Should().Be( 0 );
                errorDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( error );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var okDelegate = Substitute.For<Action<T>>();
            var errorDelegate = Substitute.For<Action<Exception>>();

            var sut = (Unsafe<T>)value;

            sut.Match( okDelegate, errorDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                errorDelegate.Verify().CallCount.Should().Be( 0 );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallErrorDelegate_WhenHasError()
        {
            var error = new Exception();
            var okDelegate = Substitute.For<Action<T>>();
            var errorDelegate = Substitute.For<Action<Exception>>();

            var sut = (Unsafe<T>)error;

            sut.Match( okDelegate, errorDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallCount.Should().Be( 0 );
                errorDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( error );
            }
        }

        [Fact]
        public void IfOk_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)value;

            var result = sut.IfOk( okDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfOk_ShouldReturnNone_WhenHasError()
        {
            var error = new Exception();
            var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

            var sut = (Unsafe<T>)error;

            var result = sut.IfOk( okDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfOk_WithAction_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var okDelegate = Substitute.For<Action<T>>();

            var sut = (Unsafe<T>)value;

            sut.IfOk( okDelegate );

            okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }

        [Fact]
        public void IfOk_WithAction_ShouldDoNothing_WhenHasError()
        {
            var error = new Exception();
            var okDelegate = Substitute.For<Action<T>>();

            var sut = (Unsafe<T>)error;

            sut.IfOk( okDelegate );

            okDelegate.Verify().CallCount.Should().Be( 0 );
        }

        [Fact]
        public void IfOkOrDefault_ShouldCallOkDelegate_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var returnedValue = Fixture.Create<T>();
            var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)value;

            var result = sut.IfOkOrDefault( okDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfOkOrDefault_ShouldReturnDefault_WhenHasError()
        {
            var error = new Exception();
            var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

            var sut = (Unsafe<T>)error;

            var result = sut.IfOkOrDefault( okDelegate );

            using ( new AssertionScope() )
            {
                okDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void IfError_ShouldCallErrorDelegate_WhenHasError()
        {
            var error = new Exception();
            var returnedValue = Fixture.Create<T>();
            var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)error;

            var result = sut.IfError( errorDelegate );

            using ( new AssertionScope() )
            {
                errorDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( error );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfError_ShouldReturnNone_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

            var sut = (Unsafe<T>)value;

            var result = sut.IfError( errorDelegate );

            using ( new AssertionScope() )
            {
                errorDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfError_WithAction_ShouldCallErrorDelegate_WhenHasError()
        {
            var error = new Exception();
            var errorDelegate = Substitute.For<Action<Exception>>();

            var sut = (Unsafe<T>)error;

            sut.IfError( errorDelegate );

            errorDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( error );
        }

        [Fact]
        public void IfError_WithAction_ShouldDoNothing_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var errorDelegate = Substitute.For<Action<Exception>>();

            var sut = (Unsafe<T>)value;

            sut.IfError( errorDelegate );

            errorDelegate.Verify().CallCount.Should().Be( 0 );
        }

        [Fact]
        public void IfErrorOrDefault_ShouldCallErrorDelegate_WhenHasError()
        {
            var error = new Exception();
            var returnedValue = Fixture.Create<T>();
            var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

            var sut = (Unsafe<T>)error;

            var result = sut.IfErrorOrDefault( errorDelegate );

            using ( new AssertionScope() )
            {
                errorDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( error );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfErrorOrDefault_ShouldReturnDefault_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

            var sut = (Unsafe<T>)value;

            var result = sut.IfErrorOrDefault( errorDelegate );

            using ( new AssertionScope() )
            {
                errorDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void UnsafeConversionOperator_FromT_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();

            var result = (Unsafe<T>)value;

            using ( new AssertionScope() )
            {
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void UnsafeConversionOperator_FromException_ShouldReturnCorrectResult()
        {
            var error = new Exception();

            var result = (Unsafe<T>)error;

            using ( new AssertionScope() )
            {
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
        }

        [Fact]
        public void EitherConversionOperator_ShouldReturnCorrectResult_WhenHasValue()
        {
            var value = Fixture.Create<T>();

            var sut = (Unsafe<T>)value;

            var result = (Either<T, Exception>)sut;

            using ( new AssertionScope() )
            {
                result.HasFirst.Should().BeTrue();
                result.First.Should().Be( value );
            }
        }

        [Fact]
        public void EitherConversionOperator_ShouldReturnCorrectResult_WhenHasError()
        {
            var error = new Exception();

            var sut = (Unsafe<T>)error;

            var result = (Either<T, Exception>)sut;

            using ( new AssertionScope() )
            {
                result.HasSecond.Should().BeTrue();
                result.Second.Should().Be( error );
            }
        }

        [Fact]
        public void UnsafeConversionOperator_FromEither_ShouldReturnCorrectResult_WhenHasFirst()
        {
            var value = Fixture.Create<T>();

            var sut = (Either<T, Exception>)value;

            var result = (Unsafe<T>)sut;

            using ( new AssertionScope() )
            {
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void UnsafeConversionOperator_FromEither_ShouldReturnCorrectResult_WhenHasSecond()
        {
            var error = new Exception();

            var sut = (Either<T, Exception>)error;

            var result = (Unsafe<T>)sut;

            using ( new AssertionScope() )
            {
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
        }

        [Fact]
        public void UnsafeConversionOperator_FromNil_ShouldReturnCorrectResult()
        {
            var result = (Unsafe<T>)Core.Functional.Nil.Instance;

            using ( new AssertionScope() )
            {
                result.IsOk.Should().BeTrue();
                result.HasError.Should().BeFalse();
                result.Value.Should().Be( default( T ) );
                result.Error.Should().BeNull();
            }
        }

        [Fact]
        public void TConversionOperator_ShouldReturnCorrectResult_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var sut = (Unsafe<T>)value;

            var result = (T)sut;

            result.Should().Be( value );
        }

        [Fact]
        public void TConversionOperator_ShouldThrowArgumentNullException_WhenHasError()
        {
            var error = new Exception();
            var sut = (Unsafe<T>)error;

            var action = Core.Functional.Lambda.Of( () => (T)sut );

            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void ExceptionConversionOperator_ShouldReturnCorrectResult_WhenHasError()
        {
            var error = new Exception();
            var sut = (Unsafe<T>)error;

            var result = (Exception)sut;

            result.Should().Be( error );
        }

        [Fact]
        public void ExceptionConversionOperator_ShouldThrowArgumentNullException_WhenHasValue()
        {
            var value = Fixture.Create<T>();
            var sut = (Unsafe<T>)value;

            var action = Core.Functional.Lambda.Of( () => (Exception)sut );

            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericUnsafeTestsData<T>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(object value1, bool isOk1, object value2, bool isOk2, bool expected)
        {
            var a = (Unsafe<T>)(isOk1 ? (T)value1 : (Exception)value1);
            var b = (Unsafe<T>)(isOk2 ? (T)value2 : (Exception)value2);

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericUnsafeTestsData<T>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(object value1, bool isOk1, object value2, bool isOk2, bool expected)
        {
            var a = (Unsafe<T>)(isOk1 ? (T)value1 : (Exception)value1);
            var b = (Unsafe<T>)(isOk2 ? (T)value2 : (Exception)value2);

            var result = a != b;

            result.Should().Be( expected );
        }

        [Fact]
        public void IReadOnlyCollectionCount_ShouldReturnOne_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = (Unsafe<T>)value;
            IReadOnlyCollection<T> collection = sut;

            var result = collection.Count;

            result.Should().Be( 1 );
        }

        [Fact]
        public void IReadOnlyCollectionCount_ShouldReturnZero_WhenHasError()
        {
            var error = new Exception();

            var sut = (Unsafe<T>)error;
            IReadOnlyCollection<T> collection = sut;

            var result = collection.Count;

            result.Should().Be( 0 );
        }

        [Fact]
        public void IEnumerableGetEnumerator_ShouldReturnEnumeratorWithOneItem_WhenHasValue()
        {
            var value = Fixture.CreateNotDefault<T>();
            var sut = (Unsafe<T>)value;
            sut.Should().BeSequentiallyEqualTo( value );
        }

        [Fact]
        public void IEnumerableGetEnumerator_ShouldReturnEmptyEnumerator_WhenHasError()
        {
            var error = new Exception();
            var sut = (Unsafe<T>)error;
            sut.Should().BeEmpty();
        }
    }
}
