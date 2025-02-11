using System.Linq;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.EitherTests;

[GenericTestClass( typeof( GenericEitherTestsData<,> ) )]
public abstract class GenericEitherTests<T1, T2> : TestsBase
    where T1 : notnull
    where T2 : notnull
{
    [Fact]
    public void Empty_ShouldHaveDefaultSecond()
    {
        var sut = Either<T1, T2>.Empty;

        Assertion.All(
                sut.HasFirst.TestFalse(),
                sut.HasSecond.TestTrue(),
                sut.First.TestEquals( default ),
                sut.Second.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;
        var expected = Hash.Default.Add( value ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;
        var expected = Hash.Default.Add( value ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEitherTestsData<T1, T2>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(object value1, bool hasFirst1, object value2, bool hasFirst2, bool expected)
    {
        var a = ( Either<T1, T2> )(hasFirst1 ? ( T1 )value1 : ( T2 )value1);
        var b = ( Either<T1, T2> )(hasFirst2 ? ( T1 )value2 : ( T2 )value2);

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetFirst_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetFirst();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetFirst_ShouldThrowValueAccessException_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var action = Lambda.Of( () => sut.GetFirst() );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Fact]
    public void GetFirstOrDefault_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.CreateNotDefault<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetFirstOrDefault();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetFirstOrDefault_ShouldReturnDefaultValue_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetFirstOrDefault();

        result.TestEquals( default ).Go();
    }

    [Fact]
    public void GetFirstOrDefault_WithValue_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.CreateNotDefault<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetFirstOrDefault( Fixture.CreateNotDefault<T1>() );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetFirstOrDefault_WithValue_ShouldReturnDefaultValue_WhenHasSecond()
    {
        var defaultValue = Fixture.CreateNotDefault<T1>();
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetFirstOrDefault( defaultValue );

        result.TestEquals( defaultValue ).Go();
    }

    [Fact]
    public void GetSecond_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetSecond();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetSecond_ShouldThrowValueAccessException_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var action = Lambda.Of( () => sut.GetSecond() );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Fact]
    public void GetSecondOrDefault_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.CreateNotDefault<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetSecondOrDefault();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetSecondOrDefault_ShouldReturnDefaultValue_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetSecondOrDefault();

        result.TestEquals( default ).Go();
    }

    [Fact]
    public void GetSecondOrDefault_WithValue_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.CreateNotDefault<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetSecondOrDefault( Fixture.CreateNotDefault<T2>() );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetSecondOrDefault_WithValue_ShouldReturnDefaultValue_WhenHasFirst()
    {
        var defaultValue = Fixture.CreateNotDefault<T2>();
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.GetSecondOrDefault( defaultValue );

        result.TestEquals( defaultValue ).Go();
    }

    [Fact]
    public void Swap_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.Swap();

        Assertion.All(
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = sut.Swap();

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Bind_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.Bind( firstDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.HasFirst.TestTrue(),
                result.First.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_ShouldNotCallFirstDelegateAndReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.Bind( firstDelegate );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void BindSecond_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.BindSecond( secondDelegate );

        Assertion.All(
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void BindSecond_ShouldNotCallSecondDelegateAndReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.BindSecond( secondDelegate );

        Assertion.All(
                secondDelegate.CallCount().TestEquals( 0 ),
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Bind_WithSecond_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.Bind( first: firstDelegate, second: secondDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                secondDelegate.CallCount().TestEquals( 0 ),
                result.HasFirst.TestTrue(),
                result.First.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_WithSecond_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.Bind( first: firstDelegate, second: secondDelegate );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );
        var secondDelegate = Substitute.For<Func<T2, T1>>().WithAnyArgs( _ => value );

        var sut = ( Either<T1, T2> )value;

        var result = sut.Match( first: firstDelegate, second: secondDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                secondDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T2>>().WithAnyArgs( _ => value );
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.Match( first: firstDelegate, second: secondDelegate );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Action<T1>>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = ( Either<T1, T2> )value;

        sut.Match( first: firstDelegate, second: secondDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                secondDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Action<T1>>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = ( Either<T1, T2> )value;

        sut.Match( first: firstDelegate, second: secondDelegate );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfFirst_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfFirst( firstDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfFirst_ShouldReturnNone_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfFirst( firstDelegate );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfFirst_WithAction_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Action<T1>>();

        var sut = ( Either<T1, T2> )value;

        sut.IfFirst( firstDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfFirst_WithAction_ShouldDoNothing_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Action<T1>>();

        var sut = ( Either<T1, T2> )value;

        sut.IfFirst( firstDelegate );

        firstDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfFirstOrDefault_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfFirstOrDefault( firstDelegate );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfFirstOrDefault_ShouldReturnDefault_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfFirstOrDefault( firstDelegate );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfFirstOrDefault_WithValue_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfFirstOrDefault( firstDelegate, Fixture.CreateNotDefault<T1>() );

        Assertion.All(
                firstDelegate.CallAt( 0 ).Exists.TestTrue(),
                firstDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfFirstOrDefault_WithValue_ShouldReturnDefault_WhenHasSecond()
    {
        var defaultValue = Fixture.CreateNotDefault<T1>();
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfFirstOrDefault( firstDelegate, defaultValue );

        Assertion.All(
                firstDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void IfSecond_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfSecond( secondDelegate );

        Assertion.All(
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfSecond_ShouldReturnNone_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfSecond( secondDelegate );

        Assertion.All(
                secondDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfSecond_WithAction_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = ( Either<T1, T2> )value;

        sut.IfSecond( secondDelegate );

        Assertion.All(
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfSecond_WithAction_ShouldDoNothing_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = ( Either<T1, T2> )value;

        sut.IfSecond( secondDelegate );

        secondDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfSecondOrDefault_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfSecondOrDefault( secondDelegate );

        Assertion.All(
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfSecondOrDefault_ShouldReturnDefault_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfSecondOrDefault( secondDelegate );

        Assertion.All(
                secondDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfSecondOrDefault_WithValue_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfSecondOrDefault( secondDelegate, Fixture.CreateNotDefault<T2>() );

        Assertion.All(
                secondDelegate.CallAt( 0 ).Exists.TestTrue(),
                secondDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfSecondOrDefault_WithValue_ShouldReturnDefault_WhenHasFirst()
    {
        var defaultValue = Fixture.CreateNotDefault<T2>();
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = ( Either<T1, T2> )value;

        var result = sut.IfSecondOrDefault( secondDelegate, defaultValue );

        Assertion.All(
                secondDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_FromT1_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T1>();

        var result = ( Either<T1, T2> )value;

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_FromT2_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T2>();

        var result = ( Either<T1, T2> )value;

        Assertion.All(
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_FromPartialEitherT1_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T1>();
        var partial = new PartialEither<T1>( value );

        var result = ( Either<T1, T2> )partial;

        Assertion.All(
                result.HasFirst.TestTrue(),
                result.First.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_FromPartialEitherT2_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T2>();
        var partial = new PartialEither<T2>( value );

        var result = ( Either<T1, T2> )partial;

        Assertion.All(
                result.HasSecond.TestTrue(),
                result.Second.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void EitherConversionOperator_FromNil_ShouldReturnCorrectResult()
    {
        var result = ( Either<T1, T2> )Nil.Instance;

        Assertion.All(
                result.HasFirst.TestFalse(),
                result.HasSecond.TestTrue(),
                result.First.TestEquals( default ),
                result.Second.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void T1ConversionOperator_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var result = ( T1 )sut;

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void T1ConversionOperator_ShouldThrowValueAccessException_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var action = Lambda.Of( () => ( T1 )sut );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Fact]
    public void T2ConversionOperator_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = ( Either<T1, T2> )value;

        var result = ( T2 )sut;

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void T2ConversionOperator_ShouldThrowValueAccessException_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = ( Either<T1, T2> )value;

        var action = Lambda.Of( () => ( T2 )sut );

        action.Test( exc => exc.TestType().Exact<ValueAccessException>() ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEitherTestsData<T1, T2>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(object value1, bool hasFirst1, object value2, bool hasFirst2, bool expected)
    {
        var a = ( Either<T1, T2> )(hasFirst1 ? ( T1 )value1 : ( T2 )value1);
        var b = ( Either<T1, T2> )(hasFirst2 ? ( T1 )value2 : ( T2 )value2);

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEitherTestsData<T1, T2>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        object value1,
        bool hasFirst1,
        object value2,
        bool hasFirst2,
        bool expected)
    {
        var a = ( Either<T1, T2> )(hasFirst1 ? ( T1 )value1 : ( T2 )value1);
        var b = ( Either<T1, T2> )(hasFirst2 ? ( T1 )value2 : ( T2 )value2);

        var result = a != b;

        result.TestEquals( expected ).Go();
    }
}
