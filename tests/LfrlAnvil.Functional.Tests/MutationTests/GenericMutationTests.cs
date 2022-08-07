using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Functional.Tests.MutationTests;

[GenericTestClass( typeof( GenericMutationTestsData<> ) )]
public abstract class GenericMutationTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void Empty_ShouldHaveDefaultValues()
    {
        var sut = Mutation<T>.Empty;

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( default( T ) );
            sut.Value.Should().Be( default( T ) );
            sut.HasChanged.Should().BeFalse();
        }
    }

    [Fact]
    public void Create_ShouldCreateCorrectResult_WhenValuesAreDifferent()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = Mutation.Create( oldValue, value );

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( oldValue );
            sut.Value.Should().Be( value );
            sut.HasChanged.Should().BeTrue();
        }
    }

    [Fact]
    public void Create_ShouldCreateCorrectResult_WhenValuesAreEqual()
    {
        var value = Fixture.Create<T>();

        var sut = Mutation.Create( value, value );

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( value );
            sut.Value.Should().Be( value );
            sut.HasChanged.Should().BeFalse();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenValuesAreDifferent()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = new Mutation<T>( oldValue, value );

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( oldValue );
            sut.Value.Should().Be( value );
            sut.HasChanged.Should().BeTrue();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenValuesAreEqual()
    {
        var value = Fixture.Create<T>();

        var sut = new Mutation<T>( value, value );

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( value );
            sut.Value.Should().Be( value );
            sut.HasChanged.Should().BeFalse();
        }
    }

    [Fact]
    public void GetHashCode_ShouldCreateCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateMany<T>( 2 ).ToList();
        var expected = Hash.Default.Add( oldValue ).Add( value ).Value;
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericMutationTestsData<T>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(T oldValue1, T value1, T oldValue2, T value2, bool expected)
    {
        var a = new Mutation<T>( oldValue1, value1 );
        var b = new Mutation<T>( oldValue2, value2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Fact]
    public void Mutate_ShouldReturnCorrectResult()
    {
        var (oldValue, value, newValue) = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Mutate( newValue );

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( value );
            result.Value.Should().Be( newValue );
        }
    }

    [Fact]
    public void Replace_ShouldReturnCorrectResult()
    {
        var (oldValue, value, newValue) = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Replace( newValue );

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( oldValue );
            result.Value.Should().Be( newValue );
        }
    }

    [Fact]
    public void Revert_ShouldReturnCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Revert();

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( oldValue );
            result.Value.Should().Be( oldValue );
        }
    }

    [Fact]
    public void Swap_ShouldReturnCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Swap();

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( value );
            result.Value.Should().Be( oldValue );
        }
    }

    [Fact]
    public void Bind_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var (returnedOldValue, returnedValue) = Fixture.CreateDistinctCollection<T>( 2 );
        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( _ => new Mutation<T>( returnedOldValue, returnedValue ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Bind( changedDelegate );

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( returnedOldValue );
            result.Value.Should().Be( returnedValue );
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
        }
    }

    [Fact]
    public void Bind_ShouldNotCallChangedDelegateAndReturnEmpty_WhenValuesHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( i => new Mutation<T>( i.ArgAt<(T, T)>( 0 ).Item1, i.ArgAt<(T, T)>( 0 ).Item2 ) );

        var sut = new Mutation<T>( value, value );

        var result = sut.Bind( changedDelegate );

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( default( T ) );
            result.Value.Should().Be( default( T ) );
            changedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void Bind_WithUnchanged_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var (returnedOldValue, returnedValue) = Fixture.CreateDistinctCollection<T>( 2 );

        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( _ => new Mutation<T>( returnedOldValue, returnedValue ) );

        var unchangedDelegate = Substitute.For<Func<T, Mutation<T>>>()
            .WithAnyArgs( i => new Mutation<T>( i.ArgAt<T>( 0 ), i.ArgAt<T>( 0 ) ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Bind( changedDelegate, unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( returnedOldValue );
            result.Value.Should().Be( returnedValue );
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
            unchangedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void Bind_WithUnchanged_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var (returnedOldValue, returnedValue) = Fixture.CreateDistinctCollection<T>( 2 );

        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( i => new Mutation<T>( i.ArgAt<(T, T)>( 0 ).Item1, i.ArgAt<(T, T)>( 0 ).Item2 ) );

        var unchangedDelegate = Substitute.For<Func<T, Mutation<T>>>()
            .WithAnyArgs( _ => new Mutation<T>( returnedOldValue, returnedValue ) );

        var sut = new Mutation<T>( value, value );

        var result = sut.Bind( changedDelegate, unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( returnedOldValue );
            result.Value.Should().Be( returnedValue );
            changedDelegate.Verify().CallCount.Should().Be( 0 );
            unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void Match_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Match( changedDelegate, unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
            unchangedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void Match_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.Match( changedDelegate, unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            changedDelegate.Verify().CallCount.Should().Be( 0 );
            unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void Match_WithAction_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var changedDelegate = Substitute.For<Action<(T, T)>>();
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( oldValue, value );

        sut.Match( changedDelegate, unchangedDelegate );

        using ( new AssertionScope() )
        {
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
            unchangedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void Match_WithAction_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Action<(T, T)>>();
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( value, value );

        sut.Match( changedDelegate, unchangedDelegate );

        using ( new AssertionScope() )
        {
            changedDelegate.Verify().CallCount.Should().Be( 0 );
            unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void IfChanged_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfChanged( changedDelegate );

        using ( new AssertionScope() )
        {
            result.Value.Should().Be( returnedValue );
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
        }
    }

    [Fact]
    public void IfChanged_ShouldReturnNone_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfChanged( changedDelegate );

        using ( new AssertionScope() )
        {
            result.HasValue.Should().BeFalse();
            changedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void IfChanged_WithAction_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var changedDelegate = Substitute.For<Action<(T, T)>>();

        var sut = new Mutation<T>( oldValue, value );

        sut.IfChanged( changedDelegate );

        changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
    }

    [Fact]
    public void IfChanged_WithAction_ShouldDoNothing_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Action<(T, T)>>();

        var sut = new Mutation<T>( value, value );

        sut.IfChanged( changedDelegate );

        changedDelegate.Verify().CallCount.Should().Be( 0 );
    }

    [Fact]
    public void IfChangedOrDefault_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfChangedOrDefault( changedDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
        }
    }

    [Fact]
    public void IfChangedOrDefault_ShouldReturnDefault_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfChangedOrDefault( changedDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( T ) );
            changedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void IfChangedOrDefault_WithValue_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfChangedOrDefault( changedDelegate, Fixture.CreateNotDefault<T>() );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            changedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( (oldValue, value) );
        }
    }

    [Fact]
    public void IfChangedOrDefault_WithValue_ShouldReturnDefault_WhenValueHasNotChanged()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfChangedOrDefault( changedDelegate, defaultValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( defaultValue );
            changedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void IfUnchanged_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfUnchanged( unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.Value.Should().Be( returnedValue );
            unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void IfUnchanged_ShouldReturnNone_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfUnchanged( unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.HasValue.Should().BeFalse();
            unchangedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void IfUnchanged_WithAction_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( value, value );

        sut.IfUnchanged( unchangedDelegate );

        unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
    }

    [Fact]
    public void IfUnchanged_WithAction_ShouldDoNothing_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( oldValue, value );

        sut.IfUnchanged( unchangedDelegate );

        unchangedDelegate.Verify().CallCount.Should().Be( 0 );
    }

    [Fact]
    public void IfUnchangedOrDefault_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void IfUnchangedOrDefault_ShouldReturnDefault_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate );

        using ( new AssertionScope() )
        {
            result.Should().Be( default( T ) );
            unchangedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void IfUnchangedOrDefault_WithValue_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate, Fixture.CreateNotDefault<T>() );

        using ( new AssertionScope() )
        {
            result.Should().Be( returnedValue );
            unchangedDelegate.Verify().CallAt( 0 ).Exists().And.ArgAt( 0 ).Should().Be( value );
        }
    }

    [Fact]
    public void IfUnchangedOrDefault_WithValue_ShouldReturnDefault_WhenValueHasChanged()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate, defaultValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( defaultValue );
            unchangedDelegate.Verify().CallCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void TConversionOperator_ShouldReturnCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateDistinctCollection<T>( 2 );
        var sut = new Mutation<T>( oldValue, value );

        var result = (T)sut;

        result.Should().Be( value );
    }

    [Fact]
    public void MutationConversionOperator_FromNil_ShouldReturnCorrectResult()
    {
        var result = (Mutation<T>)Nil.Instance;

        using ( new AssertionScope() )
        {
            result.OldValue.Should().Be( default( T ) );
            result.Value.Should().Be( default( T ) );
            result.HasChanged.Should().BeFalse();
        }
    }

    [Theory]
    [GenericMethodData( nameof( GenericMutationTestsData<T>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(T oldValue1, T value1, T oldValue2, T value2, bool expected)
    {
        var a = new Mutation<T>( oldValue1, value1 );
        var b = new Mutation<T>( oldValue2, value2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericMutationTestsData<T>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(T oldValue1, T value1, T oldValue2, T value2, bool expected)
    {
        var a = new Mutation<T>( oldValue1, value1 );
        var b = new Mutation<T>( oldValue2, value2 );

        var result = a != b;

        result.Should().Be( expected );
    }
}
