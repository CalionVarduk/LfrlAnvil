using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.TypeCastTests;

[GenericTestClass( typeof( GenericValidTypeCastTestsData<,> ) )]
public abstract class GenericValidTypeCastTests<TSource, TDestination> : GenericTypeCastTests<TSource, TDestination>
    where TSource : TDestination
    where TDestination : notnull
{
    [Theory]
    [GenericMethodData( nameof( GenericValidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(object? value1, object? value2, bool expected)
    {
        var a = value1 is null ? TypeCast<TSource, TDestination>.Empty : ( TSource )value1;
        var b = value2 is null ? TypeCast<TSource, TDestination>.Empty : ( TSource )value2;

        var result = (( object )a).Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetResult_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.GetResult();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetResultOrDefault_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.GetResultOrDefault();

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetResultOrDefault_WithValue_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.GetResultOrDefault( Fixture.CreateNotDefault<TDestination>() );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void Bind_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Bind( validDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.IsValid.TestTrue(),
                result.Result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Bind_WithInvalid_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>().WithAnyArgs( _ => returnedValue );

        var invalidDelegate = Substitute.For<Func<TSource, TypeCast<TDestination, TDestination>>>()
            .WithAnyArgs( _ => TypeCast<TDestination, TDestination>.Empty );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Bind( validDelegate, invalidDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                invalidDelegate.CallCount().TestEquals( 0 ),
                result.IsValid.TestTrue(),
                result.Result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => default! );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.Match( validDelegate, invalidDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                invalidDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Action<TDestination>>();
        var invalidDelegate = Substitute.For<Action<TSource>>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.Match( validDelegate, invalidDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                invalidDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void IfValid_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfValid( validDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.Value.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfValid_WithAction_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Action<TDestination>>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.IfValid( validDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfValidOrDefault_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfValidOrDefault( validDelegate );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfValidOrDefault_WithValue_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfValidOrDefault( validDelegate, Fixture.CreateNotDefault<TDestination>() );

        Assertion.All(
                validDelegate.CallAt( 0 ).Exists.TestTrue(),
                validDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ),
                result.TestEquals( returnedValue ) )
            .Go();
    }

    [Fact]
    public void IfInvalid_ShouldReturnNone()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => value );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfInvalid( invalidDelegate );

        Assertion.All(
                invalidDelegate.CallCount().TestEquals( 0 ),
                result.HasValue.TestFalse() )
            .Go();
    }

    [Fact]
    public void IfInvalid_WithAction_ShouldDoNothing()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Action<TSource>>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.IfInvalid( invalidDelegate );

        invalidDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfInvalidOrDefault_ShouldReturnDefault()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => value );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfInvalidOrDefault( invalidDelegate );

        Assertion.All(
                invalidDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IfInvalidOrDefault_WithValue_ShouldReturnDefault()
    {
        var defaultValue = Fixture.CreateNotDefault<TDestination>();
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => value );

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.IfInvalidOrDefault( invalidDelegate, defaultValue );

        Assertion.All(
                invalidDelegate.CallCount().TestEquals( 0 ),
                result.TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void TypeCastConversionOperator_FromTSource_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();

        var result = ( TypeCast<TSource, TDestination> )value;

        Assertion.All(
                result.IsValid.TestTrue(),
                result.IsInvalid.TestFalse(),
                result.Source.TestEquals( value ),
                result.Result.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void TypeCastConversionOperator_FromPartialTypeCast_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var partial = new PartialTypeCast<TSource>( value );

        var result = ( TypeCast<TSource, TDestination> )partial;

        Assertion.All(
                result.IsValid.TestTrue(),
                result.IsInvalid.TestFalse(),
                result.Source.TestEquals( value ),
                result.Result.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void TDestinationConversionOperator_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = ( TDestination )sut;

        result.TestEquals( value ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericValidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(object? value1, object? value2, bool expected)
    {
        var a = value1 is null ? TypeCast<TSource, TDestination>.Empty : ( TSource )value1;
        var b = value2 is null ? TypeCast<TSource, TDestination>.Empty : ( TSource )value2;

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericValidTypeCastTestsData<TSource, TDestination>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(object? value1, object? value2, bool expected)
    {
        var a = value1 is null ? TypeCast<TSource, TDestination>.Empty : ( TSource )value1;
        var b = value2 is null ? TypeCast<TSource, TDestination>.Empty : ( TSource )value2;

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnOne()
    {
        var value = Fixture.Create<TSource>();

        var sut = ( TypeCast<TSource, TDestination> )value;
        IReadOnlyCollection<TDestination> collection = sut;

        var result = collection.Count;

        result.TestEquals( 1 ).Go();
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEnumeratorWithOneItem()
    {
        var value = Fixture.Create<TSource>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        sut.TestSequence( [ value ] ).Go();
    }
}
