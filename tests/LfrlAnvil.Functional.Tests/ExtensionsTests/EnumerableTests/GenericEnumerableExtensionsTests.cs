using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.EnumerableTests;

[GenericTestClass( typeof( GenericEnumerableExtensionsTestsData<> ) )]
public abstract class GenericEnumerableExtensionsTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void SelectValues_WithMaybe_ShouldFilterOutNoneElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( Maybe.Some ).Prepend( Maybe<T>.None ).Append( Maybe<T>.None );

        var result = sut.SelectValues();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void SelectFirst_ShouldFilterOutEitherWithSecondElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( e => e.ToEither().WithSecond<Contained<T>>() )
            .Prepend( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithFirst<T>() )
            .Append( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithFirst<T>() );

        var result = sut.SelectFirst();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void SelectSecond_ShouldFilterOutEitherWithFirstElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( e => e.ToEither().WithFirst<Contained<T>>() )
            .Prepend( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithSecond<T>() )
            .Append( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithSecond<T>() );

        var result = sut.SelectSecond();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void Partition_WithEither_ShouldReturnResultWithFirstAndSecondElementsSplitBetweenCorrectCollections()
    {
        var expectedFirst = Fixture.CreateMany<T>().ToList();
        var expectedSecond = Fixture.CreateMany<T>().Select( v => new Contained<T> { Value = v } ).ToList();

        var sut = expectedFirst.Select( e => e.ToEither().WithSecond<Contained<T>>() )
            .Concat( expectedSecond.Select( e => e.ToEither().WithFirst<T>() ) );

        var (first, second) = sut.Partition();

        using ( new AssertionScope() )
        {
            first.Should().BeSequentiallyEqualTo( expectedFirst );
            second.Should().BeSequentiallyEqualTo( expectedSecond );
        }
    }

    [Fact]
    public void SelectValues_WithUnsafe_ShouldFilterOutErrorElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( e => e.ToUnsafe() ).Prepend( new Exception().ToUnsafe<T>() ).Append( new Exception().ToUnsafe<T>() );

        var result = sut.SelectValues();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void SelectErrors_ShouldFilterOutValueElements()
    {
        var expected = new List<Exception> { new Exception(), new Exception(), new Exception() };
        var sut = expected.Select( e => e.ToUnsafe<T>() )
            .Prepend( Fixture.Create<T>().ToUnsafe() )
            .Append( Fixture.Create<T>().ToUnsafe() );

        var result = sut.SelectErrors();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void Partition_WithUnsafe_ShouldReturnResultWithValueAndErrorElementsSplitBetweenCorrectCollections()
    {
        var expectedValues = Fixture.CreateMany<T>().ToList();
        var expectedErrors = new List<Exception> { new Exception(), new Exception(), new Exception() };

        var sut = expectedValues.Select( e => e.ToUnsafe() ).Concat( expectedErrors.Select( e => e.ToUnsafe<T>() ) );

        var (values, errors) = sut.Partition();

        using ( new AssertionScope() )
        {
            values.Should().BeSequentiallyEqualTo( expectedValues );
            errors.Should().BeSequentiallyEqualTo( expectedErrors );
        }
    }

    [Fact]
    public void TryMin_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryMin();
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMinData ) )]
    public void TryMin_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
    {
        var result = sut.TryMin();
        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryMax_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryMax();
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMaxData ) )]
    public void TryMax_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
    {
        var result = sut.TryMax();
        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryAggregate_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryAggregate( (_, c) => c );
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryAggregate_ShouldReturnWithValue_WhenSourceIsNotEmpty()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected );

        var result = sut.TryAggregate( (_, c) => c );

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryMaxBy_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var result = sut.TryMaxBy( c => c.Value );
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMaxData ) )]
    public void TryMaxBy_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.TryMaxBy( c => c.Value );
        result.Value!.Value.Should().Be( expected );
    }

    [Fact]
    public void TryMinBy_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var result = sut.TryMinBy( c => c.Value );
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMinData ) )]
    public void TryMinBy_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.TryMinBy( c => c.Value );
        result.Value!.Value.Should().Be( expected );
    }

    [Fact]
    public void TryFirst_ShouldReturnNone_WhenSourceIsEmpty_AndImplementsIReadOnlyList()
    {
        var sut = new List<T>();
        var result = sut.TryFirst();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasOneElement_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = new List<T> { expected };

        var result = sut.TryFirst();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasManyElements_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Prepend( expected ).ToList();

        var result = sut.TryFirst();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryFirst_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryFirst();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasOneElement()
    {
        var expected = Fixture.Create<T>();
        var sut = new[] { expected }.Select( x => x );

        var result = sut.TryFirst();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasManyElements()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Prepend( expected );

        var result = sut.TryFirst();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryLast_ShouldReturnNone_WhenSourceIsEmpty_AndImplementsIReadOnlyList()
    {
        var sut = new List<T>();
        var result = sut.TryLast();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryLast_ShouldReturnWithFirstElement_WhenSourceHasOneElement_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = new List<T> { expected };

        var result = sut.TryLast();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryLast_ShouldReturnWithLastElement_WhenSourceHasManyElements_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected ).ToList();

        var result = sut.TryLast();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryLast_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryLast();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryLast_ShouldReturnWithFirstElement_WhenSourceHasOneElement()
    {
        var expected = Fixture.Create<T>();
        var sut = new[] { expected }.Select( x => x );

        var result = sut.TryLast();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TryLast_ShouldReturnWithLastElement_WhenSourceHasManyElements()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected );

        var result = sut.TryLast();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceIsEmpty_AndImplementsIReadOnlyList()
    {
        var sut = new List<T>();
        var result = sut.TrySingle();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TrySingle_ShouldReturnWithFirstElement_WhenSourceHasOneElement_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = new List<T> { expected };

        var result = sut.TrySingle();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceHasManyElements_AndImplementsIReadOnlyList()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.TrySingle();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TrySingle();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TrySingle_ShouldReturnWithFirstElement_WhenSourceHasOneElement()
    {
        var expected = Fixture.Create<T>();
        var sut = new[] { expected }.Select( x => x );

        var result = sut.TrySingle();

        result.Value.Should().Be( expected );
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceHasManyElements()
    {
        var sut = Fixture.CreateMany<T>().Select( x => x );
        var result = sut.TrySingle();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryElementAt_ShouldReturnNone_WhenIndexIsNegative()
    {
        var sut = Fixture.CreateMany<T>();
        var result = sut.TryElementAt( -1 );
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtWithTooLargeIndexData ) )]
    public void TryElementAt_ShouldReturnNone_WhenIndexIsGreaterThanOrEqualToElementCount_AndSourceImplementsIReadOnlyList(
        IReadOnlyList<T> sut,
        int index)
    {
        var result = sut.TryElementAt( index );
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtData ) )]
    public void TryElementAt_ShouldReturnWithCorrectValue_WhenSourceHasElement_AndImplementsIReadOnlyList(
        IReadOnlyList<T> sut,
        int index,
        T expected)
    {
        var result = sut.TryElementAt( index );
        result.Value.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtWithTooLargeIndexData ) )]
    public void TryElementAt_ShouldReturnNone_WhenIndexIsGreaterThanOrEqualToElementCount(
        IEnumerable<T> sut,
        int index)
    {
        sut = sut.Select( x => x );
        var result = sut.TryElementAt( index );
        result.HasValue.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtData ) )]
    public void TryElementAt_ShouldReturnWithCorrectValue_WhenSourceHasElement(
        IEnumerable<T> sut,
        int index,
        T expected)
    {
        sut = sut.Select( x => x );
        var result = sut.TryElementAt( index );
        result.Value.Should().Be( expected );
    }
}
