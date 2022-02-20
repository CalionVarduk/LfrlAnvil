using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Functional.Tests.TypeCastTests
{
    [GenericTestClass( typeof( GenericInvalidTypeCastTestsData<,> ) )]
    public abstract class GenericInvalidTypeCastTests<TSource, TDestination> : GenericTypeCastTests<TSource, TDestination>
        where TDestination : notnull
    {
        [Theory]
        [GenericMethodData( nameof( GenericInvalidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(TSource value1, TSource value2, bool expected)
        {
            var a = (TypeCast<TSource, TDestination>)value1;
            var b = (TypeCast<TSource, TDestination>)value2;

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetResult_ShouldThrowInvalidCastException()
        {
            var value = Fixture.Create<TSource>();
            var sut = (TypeCast<TSource, TDestination>)value;

            var action = Lambda.Of( () => sut.GetResult() );

            action.Should().ThrowExactly<InvalidCastException>();
        }

        [Fact]
        public void GetResultOrDefault_ShouldReturnDefault()
        {
            var value = Fixture.Create<TSource>();
            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.GetResultOrDefault();

            result.Should().Be( default( TDestination ) );
        }

        [Fact]
        public void Bind_ShouldNotCallValidDelegateAndReturnCorrectResult()
        {
            var value = Fixture.Create<TSource>();
            var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>().WithAnyArgs( _ => default );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.Bind( validDelegate );

            using ( new AssertionScope() )
            {
                validDelegate.Verify().CallCount.Should().Be( 0 );
                result.IsInvalid.Should().BeTrue();
            }
        }

        [Fact]
        public void Bind_WithInvalid_ShouldCallInvalidDelegate()
        {
            var value = Fixture.Create<TSource>();
            var validDelegate = Substitute.For<Func<TDestination, TypeCast<TDestination, TDestination>>>()
                .WithAnyArgs( i => i.ArgAt<TDestination>( 0 ) );

            var invalidDelegate = Substitute.For<Func<TSource, TypeCast<TDestination, TDestination>>>()
                .WithAnyArgs( _ => TypeCast<TDestination, TDestination>.Empty );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.Bind( validDelegate, invalidDelegate );

            using ( new AssertionScope() )
            {
                validDelegate.Verify().CallCount.Should().Be( 0 );
                invalidDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.IsValid.Should().BeFalse();
            }
        }

        [Fact]
        public void Match_ShouldCallInvalidDelegate()
        {
            var value = Fixture.Create<TSource>();
            var returnedValue = Fixture.Create<TDestination>();
            var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( i => i.ArgAt<TDestination>( 0 ) );
            var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.Match( validDelegate, invalidDelegate );

            using ( new AssertionScope() )
            {
                validDelegate.Verify().CallCount.Should().Be( 0 );
                invalidDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void Match_WithAction_ShouldCallInvalidDelegate()
        {
            var value = Fixture.Create<TSource>();
            var validDelegate = Substitute.For<Action<TDestination>>();
            var invalidDelegate = Substitute.For<Action<TSource>>();

            var sut = (TypeCast<TSource, TDestination>)value;

            sut.Match( validDelegate, invalidDelegate );

            using ( new AssertionScope() )
            {
                validDelegate.Verify().CallCount.Should().Be( 0 );
                invalidDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            }
        }

        [Fact]
        public void IfValid_ShouldReturnNone()
        {
            var value = Fixture.Create<TSource>();
            var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => default! );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.IfValid( validDelegate );

            using ( new AssertionScope() )
            {
                validDelegate.Verify().CallCount.Should().Be( 0 );
                result.HasValue.Should().BeFalse();
            }
        }

        [Fact]
        public void IfValid_WithAction_ShouldDoNothing()
        {
            var value = Fixture.Create<TSource>();
            var validDelegate = Substitute.For<Action<TDestination>>();

            var sut = (TypeCast<TSource, TDestination>)value;

            sut.IfValid( validDelegate );

            validDelegate.Verify().CallCount.Should().Be( 0 );
        }

        [Fact]
        public void IfValidOrDefault_ShouldReturnDefault()
        {
            var value = Fixture.Create<TSource>();
            var validDelegate = Substitute.For<Func<TDestination, TDestination>>().WithAnyArgs( _ => default! );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.IfValidOrDefault( validDelegate );

            using ( new AssertionScope() )
            {
                validDelegate.Verify().CallCount.Should().Be( 0 );
                result.Should().Be( default( TDestination ) );
            }
        }

        [Fact]
        public void IfInvalid_ShouldCallInvalidDelegate()
        {
            var value = Fixture.Create<TSource>();
            var returnedValue = Fixture.Create<TDestination>();
            var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.IfInvalid( invalidDelegate );

            using ( new AssertionScope() )
            {
                invalidDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Value.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void IfInvalid_WithAction_ShouldCallInvalidDelegate()
        {
            var value = Fixture.Create<TSource>();
            var invalidDelegate = Substitute.For<Action<TSource>>();

            var sut = (TypeCast<TSource, TDestination>)value;

            sut.IfInvalid( invalidDelegate );

            invalidDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }

        [Fact]
        public void IfInvalidOrDefault_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TSource>();
            var returnedValue = Fixture.Create<TDestination>();
            var invalidDelegate = Substitute.For<Func<TSource, TDestination>>().WithAnyArgs( _ => returnedValue );

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.IfInvalidOrDefault( invalidDelegate );

            using ( new AssertionScope() )
            {
                invalidDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
                result.Should().Be( returnedValue );
            }
        }

        [Fact]
        public void TypeCastConversionOperator_FromTSource_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TSource>();

            var result = (TypeCast<TSource, TDestination>)value;

            using ( new AssertionScope() )
            {
                result.IsValid.Should().BeFalse();
                result.IsInvalid.Should().BeTrue();
                result.Source.Should().Be( value );
                result.Result.Should().Be( default( TDestination ) );
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
                result.IsValid.Should().BeFalse();
                result.IsInvalid.Should().BeTrue();
                result.Source.Should().Be( value );
                result.Result.Should().Be( default( TDestination ) );
            }
        }

        [Fact]
        public void TDestinationConversionOperator_ShouldThrowInvalidCastException()
        {
            var value = Fixture.Create<TSource>();
            var sut = (TypeCast<TSource, TDestination>)value;

            var action = Lambda.Of( () => (TDestination)sut );

            action.Should().ThrowExactly<InvalidCastException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericInvalidTypeCastTestsData<TSource, TDestination>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(TSource value1, TSource value2, bool expected)
        {
            var a = (TypeCast<TSource, TDestination>)value1;
            var b = (TypeCast<TSource, TDestination>)value2;

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericInvalidTypeCastTestsData<TSource, TDestination>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(TSource value1, TSource value2, bool expected)
        {
            var a = (TypeCast<TSource, TDestination>)value1;
            var b = (TypeCast<TSource, TDestination>)value2;

            var result = a != b;

            result.Should().Be( expected );
        }

        [Fact]
        public void IReadOnlyCollectionCount_ShouldReturnZero()
        {
            var value = Fixture.Create<TSource>();

            var sut = (TypeCast<TSource, TDestination>)value;
            IReadOnlyCollection<TDestination> collection = sut;

            var result = collection.Count;

            result.Should().Be( 0 );
        }

        [Fact]
        public void IEnumerableGetEnumerator_ShouldReturnEmptyEnumerator()
        {
            var value = Fixture.Create<TSource>();
            var sut = (TypeCast<TSource, TDestination>)value;
            sut.Should().BeEmpty();
        }
    }
}
