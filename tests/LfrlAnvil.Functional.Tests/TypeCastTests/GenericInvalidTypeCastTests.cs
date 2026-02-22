using System.Collections.Generic;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.TypeCastTests;

[GenericTestClass( typeof( GenericInvalidTypeCastTestsData<,> ) )]
public abstract class GenericInvalidTypeCastTests<TSource, TDestination> : GenericTypeCastTests<TSource, TDestination>
    where TDestination : notnull
{
    [Theory]
    [GenericMethodData( nameof( GenericInvalidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(TSource value1, TSource value2, bool expected)
    {
        var a = ( TypeCast<TSource, TDestination> )value1;
        var b = ( TypeCast<TSource, TDestination> )value2;

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetResult_ShouldThrowValueAccessException()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var action = Lambda.Of( () => sut.GetResult() );

        action.Test( exc => exc.TestType()
                .Exact<ValueAccessException>( e => e.MemberName.TestEquals( nameof( TypeCast<TSource, TDestination>.Result ) ) ) )
            .Go();
    }

    [Fact]
    public void GetResultOrDefault_ShouldReturnDefault()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.GetResultOrDefault();

        result.TestEquals( default ).Go();
    }

    [Fact]
    public void GetResultOrDefault_WithValue_ShouldReturnDefault()
    {
        var defaultValue = Fixture.CreateNotDefault<TDestination>();
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.GetResultOrDefault( defaultValue );

        result.TestEquals( defaultValue ).Go();
    }

    [Fact]
    public void Bind_ShouldNotCallValidDelegateAndReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>().WithAnyArgs( _ => default );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Bind( validDelegate );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                result.IsInvalid.TestTrue() )
            .Go();
    }

    [Fact]
    public void Bind_WithInvalid_ShouldCallInvalidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>()
            .WithAnyArgs( i => i.ArgAt<TDestination>( 0 ) );

        var invalidDelegate = Substitute.For<Func<TSource, TypeCast<TDestination, TDestination>>>()
            .WithAnyArgs( _ => TypeCast<TDestination, TDestination>.Empty );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Bind( validDelegate, invalidDelegate );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.IsValid.TestFalse() )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallInvalidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( i => i.ArgAt<TDestination>( 0 ) );
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Match( validDelegate, invalidDelegate );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallInvalidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Action<TDestination>>();
        var invalidDelegate = Substitute.For<Action<TSource>>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.Match( validDelegate, invalidDelegate );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void IfValid_ShouldReturnNone()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => default! );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfValid( validDelegate );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfValid_WithAction_ShouldDoNothing()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Action<TDestination>>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.IfValid( validDelegate );

        validDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfValidOrDefault_ShouldReturnDefault()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => default! );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfValidOrDefault( validDelegate );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfValidOrDefault_WithValue_ShouldReturnDefault()
    {
        var defaultValue = Fixture.CreateNotDefault<TDestination>();
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => default! );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfValidOrDefault( validDelegate, defaultValue );

        Assertion.All(
                validDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void IfInvalid_ShouldCallInvalidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfInvalid( invalidDelegate );

        Assertion.All(
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfInvalid_WithAction_ShouldCallInvalidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Action<TSource>>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.IfInvalid( invalidDelegate );

        Assertion.All(
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void IfInvalidOrDefault_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfInvalidOrDefault( invalidDelegate );

        Assertion.All(
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfInvalidOrDefault_WithValue_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfInvalidOrDefault( invalidDelegate, Fixture.CreateNotDefault<TDestination>() );

        Assertion.All(
                invalidDelegate.CallAt( 0 ).Exists.TestTrue(),
                invalidDelegate.CallAt( 0 ).Arguments.TestSequence( [ value ] ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void TypeCastConversionOperator_FromTSource_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();

        var result = ( TypeCast<TSource, TDestination> )value;

        Assertion.All(
                result.IsValid.TestFalse(),
                result.IsInvalid.TestTrue(),
                result.Source.TestEquals( value ),
                result.Result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TypeCastConversionOperator_FromPartialTypeCast_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var partial = new PartialTypeCast<TSource>( value );

        var result = ( TypeCast<TSource, TDestination> )partial;

        Assertion.All(
                result.IsValid.TestFalse(),
                result.IsInvalid.TestTrue(),
                result.Source.TestEquals( value ),
                result.Result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TDestinationConversionOperator_ShouldThrowValueAccessException()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var action = Lambda.Of( () => ( TDestination )sut );

        action.Test( exc => exc.TestType()
                .Exact<ValueAccessException>( e => e.MemberName.TestEquals( nameof( TypeCast<TSource, TDestination>.Result ) ) ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericInvalidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(TSource value1, TSource value2, bool expected)
    {
        var a = ( TypeCast<TSource, TDestination> )value1;
        var b = ( TypeCast<TSource, TDestination> )value2;

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericInvalidTypeCastTestsData<TSource, TDestination>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(TSource value1, TSource value2, bool expected)
    {
        var a = ( TypeCast<TSource, TDestination> )value1;
        var b = ( TypeCast<TSource, TDestination> )value2;

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnZero()
    {
        var value = Fixture.Create<TSource>();

        var sut = ( TypeCast<TSource, TDestination> )value;
        IReadOnlyCollection<TDestination> collection = sut;

        var result = collection.Count;

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEmptyEnumerator()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;
        sut.TestEmpty().Go();
    }
}
