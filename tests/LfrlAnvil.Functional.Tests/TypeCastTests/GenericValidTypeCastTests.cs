using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
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
        var a = value1 is null ? TypeCast<TSource, TDestination>.Empty : (TSource)value1;
        var b = value2 is null ? TypeCast<TSource, TDestination>.Empty : (TSource)value2;

        var result = ((object)a).Equals( b );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetResult_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.GetResult();

        result.Should().Be( value );
    }

    [Fact]
    public void GetResultOrDefault_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.GetResultOrDefault();

        result.Should().Be( value );
    }

    [Fact]
    public void GetResultOrDefault_WithValue_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.GetResultOrDefault( Fixture.CreateNotDefault<TDestination>() );

        result.Should().Be( value );
    }

    [Fact]
    public void Bind_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>()
            .WithAnyArgs( _ => returnedValue );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.Bind( validDelegate );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.IsValid.Should().BeTrue();
            result.Result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Bind_WithInvalid_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>()
            .WithAnyArgs( _ => returnedValue );

        var invalidDelegate = Substitute.For<Func<TSource, TypeCast<TDestination, TDestination>>>()
            .WithAnyArgs( _ => TypeCast<TDestination, TDestination>.Empty );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.Bind( validDelegate, invalidDelegate );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            invalidDelegate.Verify().CallCount.Should().Be( 0 );
            result.IsValid.Should().BeTrue();
            result.Result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Match_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => default! );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.Match( validDelegate, invalidDelegate );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            invalidDelegate.Verify().CallCount.Should().Be( 0 );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Match_WithAction_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Action<TDestination>>();
        var invalidDelegate = Substitute.For<Action<TSource>>();

        var sut = (TypeCast<TSource, TDestination>)value;

        sut.Match( validDelegate, invalidDelegate );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            invalidDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void IfValid_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.IfValid( validDelegate );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Value.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfValid_WithAction_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var validDelegate = Substitute.For<Action<TDestination>>();

        var sut = (TypeCast<TSource, TDestination>)value;

        sut.IfValid( validDelegate );

        validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
    }

    [Fact]
    public void IfValidOrDefault_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.IfValidOrDefault( validDelegate );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfValidOrDefault_WithValue_ShouldCallValidDelegate()
    {
        var value = Fixture.Create<TSource>();
        var returnedValue = Fixture.Create<TDestination>();
        var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => returnedValue );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.IfValidOrDefault( validDelegate, Fixture.CreateNotDefault<TDestination>() );

        using ( new AssertionScope() )
        {
            validDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfInvalid_ShouldReturnNone()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => value );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.IfInvalid( invalidDelegate );

        using ( new AssertionScope() )
        {
            invalidDelegate.Verify().CallCount.Should().Be( 0 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void IfInvalid_WithAction_ShouldDoNothing()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Action<TSource>>();

        var sut = (TypeCast<TSource, TDestination>)value;

        sut.IfInvalid( invalidDelegate );

        invalidDelegate.Verify().CallCount.Should().Be( 0 );
    }

    [Fact]
    public void IfInvalidOrDefault_ShouldReturnDefault()
    {
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => value );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.IfInvalidOrDefault( invalidDelegate );

        using ( new AssertionScope() )
        {
            invalidDelegate.Verify().CallCount.Should().Be( 0 );
            result.Should().Be( default( TDestination ) );
        }
    }

    [Fact]
    public void IfInvalidOrDefault_WithValue_ShouldReturnDefault()
    {
        var defaultValue = Fixture.CreateNotDefault<TDestination>();
        var value = Fixture.Create<TSource>();
        var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => value );

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.IfInvalidOrDefault( invalidDelegate, defaultValue );

        using ( new AssertionScope() )
        {
            invalidDelegate.Verify().CallCount.Should().Be( 0 );
            result.Should().Be( defaultValue );
        }
    }

    [Fact]
    public void TypeCastConversionOperator_FromTSource_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();

        var result = (TypeCast<TSource, TDestination>)value;

        using ( new AssertionScope() )
        {
            result.IsValid.Should().BeTrue();
            result.IsInvalid.Should().BeFalse();
            result.Source.Should().Be( value );
            result.Result.Should().Be( value );
        }
    }

    [Fact]
    public void TypeCastConversionOperator_FromPartialTypeCast_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var partial = new PartialTypeCast<TSource>( value );

        var result = (TypeCast<TSource, TDestination>)partial;

        using ( new AssertionScope() )
        {
            result.IsValid.Should().BeTrue();
            result.IsInvalid.Should().BeFalse();
            result.Source.Should().Be( value );
            result.Result.Should().Be( value );
        }
    }

    [Fact]
    public void TDestinationConversionOperator_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<TSource>();
        var sut = (TypeCast<TSource, TDestination>)value;

        var result = (TDestination)sut;

        result.Should().Be( value );
    }

    [Theory]
    [GenericMethodData( nameof( GenericValidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(object? value1, object? value2, bool expected)
    {
        var a = value1 is null ? TypeCast<TSource, TDestination>.Empty : (TSource)value1;
        var b = value2 is null ? TypeCast<TSource, TDestination>.Empty : (TSource)value2;

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericValidTypeCastTestsData<TSource, TDestination>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(object? value1, object? value2, bool expected)
    {
        var a = value1 is null ? TypeCast<TSource, TDestination>.Empty : (TSource)value1;
        var b = value2 is null ? TypeCast<TSource, TDestination>.Empty : (TSource)value2;

        var result = a != b;

        result.Should().Be( expected );
    }

    [Fact]
    public void IReadOnlyCollectionCount_ShouldReturnOne()
    {
        var value = Fixture.Create<TSource>();

        var sut = (TypeCast<TSource, TDestination>)value;
        IReadOnlyCollection<TDestination> collection = sut;

        var result = collection.Count;

        result.Should().Be( 1 );
    }

    [Fact]
    public void IEnumerableGetEnumerator_ShouldReturnEnumeratorWithOneItem()
    {
        var value = Fixture.Create<TSource>();

        var sut = (TypeCast<TSource, TDestination>)value;

        sut.Should().BeSequentiallyEqualTo( value );
    }
}
