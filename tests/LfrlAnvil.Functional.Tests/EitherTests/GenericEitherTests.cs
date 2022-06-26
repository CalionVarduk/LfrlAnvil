using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

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

        using ( new AssertionScope() )
        {
            sut.HasFirst.Should().BeFalse();
            sut.HasSecond.Should().BeTrue();
            sut.First.Should().Be( default( T1 ) );
            sut.Second.Should().Be( default( T2 ) );
        }
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;
        var expected = Hash.Default.Add( value ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;
        var expected = Hash.Default.Add( value ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericEitherTestsData<T1, T2>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(object value1, bool hasFirst1, object value2, bool hasFirst2, bool expected)
    {
        var a = (Either<T1, T2>)(hasFirst1 ? (T1)value1 : (T2)value1);
        var b = (Either<T1, T2>)(hasFirst2 ? (T1)value2 : (T2)value2);

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetFirst_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;

        var result = sut.GetFirst();

        result.Should().Be( value );
    }

    [Fact]
    public void GetFirst_ShouldThrowValueAccessException_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;

        var action = Lambda.Of( () => sut.GetFirst() );

        action.Should().ThrowExactly<ValueAccessException>().AndMatch( e => e.MemberName == nameof( Either<T1, T2>.First ) );
    }

    [Fact]
    public void GetFirstOrDefault_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.CreateNotDefault<T1>();
        var sut = (Either<T1, T2>)value;

        var result = sut.GetFirstOrDefault();

        result.Should().Be( value );
    }

    [Fact]
    public void GetFirstOrDefault_ShouldReturnDefaultValue_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;

        var result = sut.GetFirstOrDefault();

        result.Should().Be( default( T1 ) );
    }

    [Fact]
    public void GetSecond_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;

        var result = sut.GetSecond();

        result.Should().Be( value );
    }

    [Fact]
    public void GetSecond_ShouldThrowValueAccessException_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;

        var action = Lambda.Of( () => sut.GetSecond() );

        action.Should().ThrowExactly<ValueAccessException>().AndMatch( e => e.MemberName == nameof( Either<T1, T2>.Second ) );
    }

    [Fact]
    public void GetSecondOrDefault_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.CreateNotDefault<T2>();
        var sut = (Either<T1, T2>)value;

        var result = sut.GetSecondOrDefault();

        result.Should().Be( value );
    }

    [Fact]
    public void GetSecondOrDefault_ShouldReturnDefaultValue_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;

        var result = sut.GetSecondOrDefault();

        result.Should().Be( default( T2 ) );
    }

    [Fact]
    public void Swap_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;

        var result = sut.Swap();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Swap_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;

        var result = sut.Swap();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Bind_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.Bind( firstDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Bind_ShouldNotCallFirstDelegateAndReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.Bind( firstDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void BindSecond_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.BindSecond( secondDelegate );

        using ( new AssertionScope() )
        {
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void BindSecond_ShouldNotCallSecondDelegateAndReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.BindSecond( secondDelegate );

        using ( new AssertionScope() )
        {
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Bind_WithSecond_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.Bind( first: firstDelegate, second: secondDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Bind_WithSecond_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, Either<T1, T2>>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );
        var secondDelegate = Substitute.For<Func<T2, Either<T1, T2>>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.Bind( first: firstDelegate, second: secondDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Match_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );
        var secondDelegate = Substitute.For<Func<T2, T1>>().WithAnyArgs( _ => value );

        var sut = (Either<T1, T2>)value;

        var result = sut.Match( first: firstDelegate, second: secondDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Match_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T2>>().WithAnyArgs( _ => value );
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.Match( first: firstDelegate, second: secondDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void Match_WithAction_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Action<T1>>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = (Either<T1, T2>)value;

        sut.Match( first: firstDelegate, second: secondDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            secondDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void Match_WithAction_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Action<T1>>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = (Either<T1, T2>)value;

        sut.Match( first: firstDelegate, second: secondDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void IfFirst_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfFirst( firstDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Value.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfFirst_ShouldReturnNone_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfFirst( firstDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void IfFirst_WithAction_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Action<T1>>();

        var sut = (Either<T1, T2>)value;

        sut.IfFirst( firstDelegate );

        firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
    }

    [Fact]
    public void IfFirst_WithAction_ShouldDoNothing_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Action<T1>>();

        var sut = (Either<T1, T2>)value;

        sut.IfFirst( firstDelegate );

        firstDelegate.Verify().CallCount.Should().Be( 0 );
    }

    [Fact]
    public void IfFirstOrDefault_ShouldCallFirstDelegate_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var returnedValue = Fixture.Create<T1>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfFirstOrDefault( firstDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfFirstOrDefault_ShouldReturnDefault_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var firstDelegate = Substitute.For<Func<T1, T1>>().WithAnyArgs( i => i.ArgAt<T1>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfFirstOrDefault( firstDelegate );

        using ( new AssertionScope() )
        {
            firstDelegate.Verify().CallCount.Should().Be( 0 );
            result.Should().Be( default( T1 ) );
        }
    }

    [Fact]
    public void IfSecond_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfSecond( secondDelegate );

        using ( new AssertionScope() )
        {
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Value.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfSecond_ShouldReturnNone_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfSecond( secondDelegate );

        using ( new AssertionScope() )
        {
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void IfSecond_WithAction_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = (Either<T1, T2>)value;

        sut.IfSecond( secondDelegate );

        secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
    }

    [Fact]
    public void IfSecond_WithAction_ShouldDoNothing_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Action<T2>>();

        var sut = (Either<T1, T2>)value;

        sut.IfSecond( secondDelegate );

        secondDelegate.Verify().CallCount.Should().Be( 0 );
    }

    [Fact]
    public void IfSecondOrDefault_ShouldCallSecondDelegate_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var returnedValue = Fixture.Create<T2>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( _ => returnedValue );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfSecondOrDefault( secondDelegate );

        using ( new AssertionScope() )
        {
            secondDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
            result.Should().Be( returnedValue );
        }
    }

    [Fact]
    public void IfSecondOrDefault_ShouldReturnDefault_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var secondDelegate = Substitute.For<Func<T2, T2>>().WithAnyArgs( i => i.ArgAt<T2>( 0 ) );

        var sut = (Either<T1, T2>)value;

        var result = sut.IfSecondOrDefault( secondDelegate );

        using ( new AssertionScope() )
        {
            secondDelegate.Verify().CallCount.Should().Be( 0 );
            result.Should().Be( default( T2 ) );
        }
    }

    [Fact]
    public void EitherConversionOperator_FromT1_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T1>();

        var result = (Either<T1, T2>)value;

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void EitherConversionOperator_FromT2_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T2>();

        var result = (Either<T1, T2>)value;

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void EitherConversionOperator_FromPartialEitherT1_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T1>();
        var partial = new PartialEither<T1>( value );

        var result = (Either<T1, T2>)partial;

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void EitherConversionOperator_FromPartialEitherT2_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T2>();
        var partial = new PartialEither<T2>( value );

        var result = (Either<T1, T2>)partial;

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void EitherConversionOperator_FromNil_ShouldReturnCorrectResult()
    {
        var result = (Either<T1, T2>)Nil.Instance;

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeFalse();
            result.HasSecond.Should().BeTrue();
            result.First.Should().Be( default( T1 ) );
            result.Second.Should().Be( default( T2 ) );
        }
    }

    [Fact]
    public void T1ConversionOperator_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;

        var result = (T1)sut;

        result.Should().Be( value );
    }

    [Fact]
    public void T1ConversionOperator_ShouldThrowValueAccessException_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;

        var action = Lambda.Of( () => (T1)sut );

        action.Should().ThrowExactly<ValueAccessException>().AndMatch( e => e.MemberName == nameof( Either<T1, T2>.First ) );
    }

    [Fact]
    public void T2ConversionOperator_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = (Either<T1, T2>)value;

        var result = (T2)sut;

        result.Should().Be( value );
    }

    [Fact]
    public void T2ConversionOperator_ShouldThrowValueAccessException_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = (Either<T1, T2>)value;

        var action = Lambda.Of( () => (T2)sut );

        action.Should().ThrowExactly<ValueAccessException>().AndMatch( e => e.MemberName == nameof( Either<T1, T2>.Second ) );
    }

    [Theory]
    [GenericMethodData( nameof( GenericEitherTestsData<T1, T2>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(object value1, bool hasFirst1, object value2, bool hasFirst2, bool expected)
    {
        var a = (Either<T1, T2>)(hasFirst1 ? (T1)value1 : (T2)value1);
        var b = (Either<T1, T2>)(hasFirst2 ? (T1)value2 : (T2)value2);

        var result = a == b;

        result.Should().Be( expected );
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
        var a = (Either<T1, T2>)(hasFirst1 ? (T1)value1 : (T2)value1);
        var b = (Either<T1, T2>)(hasFirst2 ? (T1)value2 : (T2)value2);

        var result = a != b;

        result.Should().Be( expected );
    }
}