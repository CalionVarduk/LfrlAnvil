using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.ErraticTests;

[GenericTestClass( typeof( GenericErraticTestsData<> ) )]
public abstract class GenericErraticTests<T> : TestsBase
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

        var result = Erratic.Try( Action );

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Try_ShouldReturnCorrectResult_WhenDelegateThrows()
    {
        var error = new Exception();

        T Action()
        {
            throw error;
        }

        var result = Erratic.Try( Action );

        Assertion.All(
                result.HasError.TestTrue(),
                result.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void Empty_ShouldHaveDefaultValue()
    {
        var sut = Erratic<T>.Empty;

        Assertion.All(
                sut.IsOk.TestTrue(),
                sut.HasError.TestFalse(),
                sut.Value.TestEquals( default ),
                sut.Error.TestNull() )
            .Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var sut = ( Erratic<T> )value;
        var expected = Hash.Default.Add( value ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();
        var sut = ( Erratic<T> )error;
        var expected = Hash.Default.Add( error ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericErraticTestsData<T>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(object value1, bool isOk1, object value2, bool isOk2, bool expected)
    {
        var a = ( Erratic<T> )(isOk1 ? ( T )value1 : ( Exception )value1);
        var b = ( Erratic<T> )(isOk2 ? ( T )value2 : ( Exception )value2);

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetValue_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        IErratic sut = ( Erratic<T> )value;

        var result = sut.GetValue();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetValue_ShouldThrowValueAccessException_WhenHasError()
    {
        var error = new Exception();
        IErratic sut = ( Erratic<T> )error;

        var action = Lambda.Of( () => sut.GetValue() );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        IErratic sut = ( Erratic<T> )value;

        var result = sut.GetValueOrDefault();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDefaultValue_WhenHasError()
    {
        var error = new Exception();
        IErratic sut = ( Erratic<T> )error;

        var result = sut.GetValueOrDefault();

        result.TestEquals( default( T ) ).Go();
    }

    [Fact]
    public void GetValueOrDefault_WithValue_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = ( Erratic<T> )value;

        var result = sut.GetValueOrDefault( Fixture.CreateNotDefault<T>() );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetValueOrDefault_WithValue_ShouldReturnDefaultValue_WhenHasError()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var error = new Exception();
        var sut = ( Erratic<T> )error;

        var result = sut.GetValueOrDefault( defaultValue );

        result.TestEquals( defaultValue ).Go();
    }

    [Fact]
    public void GetError_ShouldReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();
        IErratic sut = ( Erratic<T> )error;

        var result = sut.GetError();

        result.TestEquals( error ).Go();
    }

    [Fact]
    public void GetError_ShouldThrowValueAccessException_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        IErratic sut = ( Erratic<T> )value;

        var action = Lambda.Of( () => sut.GetError() );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Fact]
    public void GetErrorOrDefault_ShouldReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();
        IErratic sut = ( Erratic<T> )error;

        var result = sut.GetErrorOrDefault();

        result.TestEquals( error ).Go();
    }

    [Fact]
    public void GetErrorOrDefault_ShouldReturnNull_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        IErratic sut = ( Erratic<T> )value;

        var result = sut.GetErrorOrDefault();

        result.TestNull().Go();
    }

    [Fact]
    public void Bind_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, Erratic<T>>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )value;

        var result = sut.Bind( okDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.IsOk.TestTrue(),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_ShouldNotCallOkDelegateAndReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();
        var okDelegate = Substitute.For<Func<T, Erratic<T>>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = ( Erratic<T> )error;

        var result = sut.Bind( okDelegate );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                result.HasError.TestTrue(),
                result.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void Bind_WithError_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, Erratic<T>>>().WithAnyArgs( _ => returnedValue );
        var errorDelegate = Substitute.For<Func<Exception, Erratic<T>>>().WithAnyArgs( _ => value );

        var sut = ( Erratic<T> )value;

        var result = sut.Bind( okDelegate, errorDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                errorDelegate.CallCount().TestEquals( 0 ),
                result.IsOk.TestTrue(),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_WithError_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, Erratic<T>>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var errorDelegate = Substitute.For<Func<Exception, Erratic<T>>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )error;

        var result = sut.Bind( okDelegate, errorDelegate );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ),
                result.IsOk.TestTrue(),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

        var sut = ( Erratic<T> )value;

        var result = sut.Match( okDelegate, errorDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                errorDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )error;

        var result = sut.Match( okDelegate, errorDelegate );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var okDelegate = Substitute.For<Action<T>>();
        var errorDelegate = Substitute.For<Action<Exception>>();

        var sut = ( Erratic<T> )value;

        sut.Match( okDelegate, errorDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                errorDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var okDelegate = Substitute.For<Action<T>>();
        var errorDelegate = Substitute.For<Action<Exception>>();

        var sut = ( Erratic<T> )error;

        sut.Match( okDelegate, errorDelegate );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void IfOk_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )value;

        var result = sut.IfOk( okDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfOk_ShouldReturnNone_WhenHasError()
    {
        var error = new Exception();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = ( Erratic<T> )error;

        var result = sut.IfOk( okDelegate );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfOk_WithAction_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var okDelegate = Substitute.For<Action<T>>();

        var sut = ( Erratic<T> )value;

        sut.IfOk( okDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfOk_WithAction_ShouldDoNothing_WhenHasError()
    {
        var error = new Exception();
        var okDelegate = Substitute.For<Action<T>>();

        var sut = ( Erratic<T> )error;

        sut.IfOk( okDelegate );

        okDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfOkOrDefault_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )value;

        var result = sut.IfOkOrDefault( okDelegate );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfOkOrDefault_ShouldReturnDefault_WhenHasError()
    {
        var error = new Exception();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = ( Erratic<T> )error;

        var result = sut.IfOkOrDefault( okDelegate );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfOkOrDefault_WithValue_ShouldCallOkDelegate_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )value;

        var result = sut.IfOkOrDefault( okDelegate, Fixture.CreateNotDefault<T>() );

        Assertion.All(
                okDelegate.CallAt( 0 ).Exists.TestTrue(),
                okDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfOkOrDefault_WithValue_ShouldReturnDefault_WhenHasError()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var error = new Exception();
        var okDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = ( Erratic<T> )error;

        var result = sut.IfOkOrDefault( okDelegate, defaultValue );

        Assertion.All(
                okDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void IfError_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var returnedValue = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )error;

        var result = sut.IfError( errorDelegate );

        Assertion.All(
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfError_ShouldReturnNone_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

        var sut = ( Erratic<T> )value;

        var result = sut.IfError( errorDelegate );

        Assertion.All(
                errorDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfError_WithAction_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var errorDelegate = Substitute.For<Action<Exception>>();

        var sut = ( Erratic<T> )error;

        sut.IfError( errorDelegate );

        Assertion.All(
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void IfError_WithAction_ShouldDoNothing_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Action<Exception>>();

        var sut = ( Erratic<T> )value;

        sut.IfError( errorDelegate );

        errorDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfErrorOrDefault_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var returnedValue = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )error;

        var result = sut.IfErrorOrDefault( errorDelegate );

        Assertion.All(
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfErrorOrDefault_ShouldReturnDefault_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

        var sut = ( Erratic<T> )value;

        var result = sut.IfErrorOrDefault( errorDelegate );

        Assertion.All(
                errorDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfErrorOrDefault_WithValue_ShouldCallErrorDelegate_WhenHasError()
    {
        var error = new Exception();
        var returnedValue = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Erratic<T> )error;

        var result = sut.IfErrorOrDefault( errorDelegate, Fixture.CreateNotDefault<T>() );

        Assertion.All(
                errorDelegate.CallAt( 0 ).Exists.TestTrue(),
                errorDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( error ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfErrorOrDefault_WithValue_ShouldReturnDefault_WhenHasValue()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var value = Fixture.Create<T>();
        var errorDelegate = Substitute.For<Func<Exception, T>>().WithAnyArgs( _ => value );

        var sut = ( Erratic<T> )value;

        var result = sut.IfErrorOrDefault( errorDelegate, defaultValue );

        Assertion.All(
                errorDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void ErraticConversionOperator_FromT_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();

        var result = ( Erratic<T> )value;

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ErraticConversionOperator_FromException_ShouldReturnCorrectResult()
    {
        var error = new Exception();

        var result = ( Erratic<T> )error;

        Assertion.All(
                result.HasError.TestTrue(),
                result.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.Create<T>();

        var sut = ( Erratic<T> )value;

        var result = ( Either<T, Exception> )sut;

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_ShouldReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();

        var sut = ( Erratic<T> )error;

        var result = ( Either<T, Exception> )sut;

        Assertion.All(
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void ErraticConversionOperator_FromEither_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T>();

        var sut = ( Either<T, Exception> )value;

        var result = ( Erratic<T> )sut;

        Assertion.All(
                result.IsOk.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void ErraticConversionOperator_FromEither_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var error = new Exception();

        var sut = ( Either<T, Exception> )error;

        var result = ( Erratic<T> )sut;

        Assertion.All(
                result.HasError.TestTrue(),
                result.Error.TestEquals( error ) )
            .Go();
    }

    [Fact]
    public void ErraticConversionOperator_FromNil_ShouldReturnCorrectResult()
    {
        var result = ( Erratic<T> )Nil.Instance;

        Assertion.All(
                result.IsOk.TestTrue(),
                result.HasError.TestFalse(),
                result.Value.TestEquals( default ),
                result.Error.TestNull() )
            .Go();
    }

    [Fact]
    public void TConversionOperator_ShouldReturnCorrectResult_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var sut = ( Erratic<T> )value;

        var result = ( T )sut;

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void TConversionOperator_ShouldThrowValueAccessException_WhenHasError()
    {
        var error = new Exception();
        var sut = ( Erratic<T> )error;

        var action = Lambda.Of( () => ( T )sut );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Fact]
    public void ExceptionConversionOperator_ShouldReturnCorrectResult_WhenHasError()
    {
        var error = new Exception();
        var sut = ( Erratic<T> )error;

        var result = ( Exception )sut;

        result.TestEquals( error ).Go();
    }

    [Fact]
    public void ExceptionConversionOperator_ShouldThrowValueAccessException_WhenHasValue()
    {
        var value = Fixture.Create<T>();
        var sut = ( Erratic<T> )value;

        var action = Lambda.Of( () => ( Exception )sut );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericErraticTestsData<T>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(object value1, bool isOk1, object value2, bool isOk2, bool expected)
    {
        var a = ( Erratic<T> )(isOk1 ? ( T )value1 : ( Exception )value1);
        var b = ( Erratic<T> )(isOk2 ? ( T )value2 : ( Exception )value2);

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericErraticTestsData<T>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(object value1, bool isOk1, object value2, bool isOk2, bool expected)
    {
        var a = ( Erratic<T> )(isOk1 ? ( T )value1 : ( Exception )value1);
        var b = ( Erratic<T> )(isOk2 ? ( T )value2 : ( Exception )value2);

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnOne_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = ( Erratic<T> )value;
        IReadOnlyCollection<T> collection = sut;

        var result = collection.Count;

        result.TestEquals( 1 ).Go();
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnZero_WhenHasError()
    {
        var error = new Exception();

        var sut = ( Erratic<T> )error;
        IReadOnlyCollection<T> collection = sut;

        var result = collection.Count;

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEnumeratorWithOneItem_WhenHasValue()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = ( Erratic<T> )value;
        sut.TestSequence( [ value ] ).Go();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEmptyEnumerator_WhenHasError()
    {
        var error = new Exception();
        var sut = ( Erratic<T> )error;
        sut.TestEmpty().Go();
    }
}
