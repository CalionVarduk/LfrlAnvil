using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.ChainTests;

public abstract class GenericChainTests<T> : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptyChainThatCanBeExtended()
    {
        var sut = default( Chain<T> );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
        }
    }

    [Fact]
    public void Empty_ShouldReturnEmptyChainThatCanBeExtended()
    {
        var sut = Chain<T>.Empty;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
        }
    }

    [Fact]
    public void Create_WithOneValue_ShouldReturnCorrectChain()
    {
        var value = Fixture.Create<T>();
        var sut = Chain.Create( value );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Create_WithMultipleValues_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( values.Count );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Ctor_WithOneValue_ShouldReturnCorrectChain()
    {
        var value = Fixture.Create<T>();
        var sut = new Chain<T>( value );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Ctor_WithMultipleValues_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = new Chain<T>( values.AsEnumerable() );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( values.Count );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Ctor_WithEmptyEnumerable_ShouldReturnEmptyChain()
    {
        var sut = new Chain<T>( Enumerable.Empty<T>() );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Extend_WithOneValue_ShouldReturnCorrectChain_WhenChainIsEmpty()
    {
        var value = Fixture.Create<T>();
        var sut = Chain<T>.Empty;

        var result = sut.Extend( value );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 1 );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Extend_WithOneValue_ShouldNotModifyOriginalChain_WhenChainIsEmpty()
    {
        var value = Fixture.Create<T>();
        var sut = Chain<T>.Empty;

        var _ = sut.Extend( value );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Extend_WithOneValue_ShouldReturnCorrectChain_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 4 );
        var initialValues = allValues.Take( 3 );
        var value = allValues[^1];
        var sut = Chain.Create( initialValues );

        var result = sut.Extend( value );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( allValues.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( allValues );
        }
    }

    [Fact]
    public void Extend_WithOneValue_ShouldMakeOriginalChainNonExtendable_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 4 );
        var initialValues = allValues.Take( 3 ).ToList();
        var value = allValues[^1];
        var sut = Chain.Create( initialValues.AsEnumerable() );

        var _ = sut.Extend( value );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( initialValues.Count );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeFalse();
            sut.Should().BeSequentiallyEqualTo( initialValues );
        }
    }

    [Fact]
    public void Extend_WithOneValue_ShouldThrowInvalidOperationException_WhenChainHasAlreadyBeenExtended()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( values[0] );
        var _ = sut.Extend( values[1] );

        var action = Lambda.Of( () => sut.Extend( values[2] ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldThrowInvalidOperationException_WhenChainIsAttachedToAnotherChain()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( values[0] );
        var _ = Chain.Create( values[1] ).Extend( sut );

        var action = Lambda.Of( () => sut.Extend( values[2] ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldReturnCorrectChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain<T>.Empty;

        var result = sut.Extend( values );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( values.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldNotModifyOriginalChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain<T>.Empty;

        var _ = sut.Extend( values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldReturnCorrectChain_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues );

        var result = sut.Extend( values );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( allValues.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( allValues );
        }
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldMakeOriginalChainNonExtendable_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 ).ToList();
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );

        var _ = sut.Extend( values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( initialValues.Count );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeFalse();
            sut.Should().BeSequentiallyEqualTo( initialValues );
        }
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldReturnOriginalChain_WhenEnumerableIsEmpty()
    {
        var initialValues = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );

        var result = sut.Extend( Enumerable.Empty<T>() );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( initialValues.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( initialValues );
            sut.IsExtendable.Should().BeTrue();
        }
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldThrowInvalidOperationException_WhenChainHasAlreadyBeenExtended()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        var _ = sut.Extend( values[1] );

        var action = Lambda.Of( () => sut.Extend( values.Skip( 2 ) ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldThrowInvalidOperationException_WhenChainIsAttachedToAnotherChain()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        var _ = Chain.Create( values[1] ).Extend( sut );

        var action = Lambda.Of( () => sut.Extend( values.Skip( 2 ) ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain<T>.Empty;
        var other = Chain.Create( values.AsEnumerable() );

        var result = sut.Extend( other );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( values.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldNotModifyOriginalChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain<T>.Empty;
        var other = Chain.Create( values.AsEnumerable() );

        var _ = sut.Extend( other );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeTrue();
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldNotModifyOtherChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain<T>.Empty;
        var other = Chain.Create( values.AsEnumerable() );

        var _ = sut.Extend( other );

        using ( new AssertionScope() )
        {
            other.Count.Should().Be( values.Count );
            other.IsAttached.Should().BeFalse();
            other.IsExtendable.Should().BeTrue();
            other.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values );

        var result = sut.Extend( other );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( allValues.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( allValues );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldMakeOriginalChainNonExtendable_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 ).ToList();
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );
        var other = Chain.Create( values );

        var _ = sut.Extend( other );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( initialValues.Count );
            sut.IsAttached.Should().BeFalse();
            sut.IsExtendable.Should().BeFalse();
            sut.Should().BeSequentiallyEqualTo( initialValues );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldMakeOtherChainAttached_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 ).ToList();
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values.AsEnumerable() );

        var _ = sut.Extend( other );

        using ( new AssertionScope() )
        {
            other.Count.Should().Be( values.Count );
            other.IsAttached.Should().BeTrue();
            other.IsExtendable.Should().BeFalse();
            other.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnOriginalChain_WhenOtherChainIsEmpty()
    {
        var initialValues = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );

        var result = sut.Extend( Chain<T>.Empty );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( initialValues.Count );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( initialValues );
            sut.IsExtendable.Should().BeTrue();
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChainMarkedAsExtended_WhenOtherChainIsAlreadyExtended()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 ).Take( 2 );
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values );
        var _ = other.Extend( allValues[^1] );

        var result = sut.Extend( other );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( allValues.Count - 1 );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeFalse();
            result.Should().BeSequentiallyEqualTo( allValues.SkipLast( 1 ) );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenOtherChainIsAlreadyAttached()
    {
        var allValues = Fixture.CreateDistinctCollection<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 ).Take( 2 );
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values );
        var _ = Chain.Create( allValues[^1] ).Extend( other );

        var result = sut.Extend( other );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( allValues.Count - 1 );
            result.IsAttached.Should().BeFalse();
            result.IsExtendable.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( allValues.SkipLast( 1 ) );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenAttachingSelf()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        var result = sut.Extend( sut );

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( values.Count * 2 );
            result.IsAttached.Should().BeTrue();
            result.IsExtendable.Should().BeFalse();
            result.Should().BeSequentiallyEqualTo( values.Concat( values ) );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldModifyOriginalChain_WhenAttachingSelf()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        var _ = sut.Extend( sut );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( values.Count );
            sut.IsAttached.Should().BeTrue();
            sut.IsExtendable.Should().BeFalse();
            sut.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void Extend_WithChain_ShouldThrowInvalidOperationException_WhenChainHasAlreadyBeenExtended()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        var _ = sut.Extend( values[1] );

        var action = Lambda.Of( () => sut.Extend( Chain.Create( values.Skip( 2 ) ) ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Extend_WithChain_ShouldThrowInvalidOperationException_WhenChainIsAttachedToAnotherChain()
    {
        var values = Fixture.CreateDistinctCollection<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        var _ = Chain.Create( values[1] ).Extend( sut );

        var action = Lambda.Of( () => sut.Extend( Chain.Create( values.Skip( 2 ) ) ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }
}
