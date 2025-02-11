using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions.Attributes;

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

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void SelectFirst_ShouldFilterOutEitherWithSecondElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( e => e.ToEither().WithSecond<Contained<T>>() )
            .Prepend( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithFirst<T>() )
            .Append( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithFirst<T>() );

        var result = sut.SelectFirst();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void SelectSecond_ShouldFilterOutEitherWithFirstElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( e => e.ToEither().WithFirst<Contained<T>>() )
            .Prepend( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithSecond<T>() )
            .Append( new Contained<T> { Value = Fixture.Create<T>() }.ToEither().WithSecond<T>() );

        var result = sut.SelectSecond();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void Partition_WithEither_ShouldReturnResultWithFirstAndSecondElementsSplitBetweenCorrectCollections()
    {
        var expectedFirst = Fixture.CreateMany<T>().ToList();
        var expectedSecond = Fixture.CreateMany<T>().Select( v => new Contained<T> { Value = v } ).ToList();

        var sut = expectedFirst.Select( e => e.ToEither().WithSecond<Contained<T>>() )
            .Concat( expectedSecond.Select( e => e.ToEither().WithFirst<T>() ) );

        var (first, second) = sut.Partition();

        Assertion.All(
                first.TestSequence( expectedFirst ),
                second.TestSequence( expectedSecond ) )
            .Go();
    }

    [Fact]
    public void SelectValues_WithErratic_ShouldFilterOutErrorElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( e => e.ToErratic() ).Prepend( new Exception().ToErratic<T>() ).Append( new Exception().ToErratic<T>() );

        var result = sut.SelectValues();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void SelectErrors_ShouldFilterOutValueElements()
    {
        var expected = new List<Exception>
        {
            new Exception(),
            new Exception(),
            new Exception()
        };

        var sut = expected.Select( e => e.ToErratic<T>() )
            .Prepend( Fixture.Create<T>().ToErratic() )
            .Append( Fixture.Create<T>().ToErratic() );

        var result = sut.SelectErrors();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void Partition_WithErratic_ShouldReturnResultWithValueAndErrorElementsSplitBetweenCorrectCollections()
    {
        var expectedValues = Fixture.CreateMany<T>().ToList();
        var expectedErrors = new List<Exception>
        {
            new Exception(),
            new Exception(),
            new Exception()
        };

        var sut = expectedValues.Select( e => e.ToErratic() ).Concat( expectedErrors.Select( e => e.ToErratic<T>() ) );

        var (values, errors) = sut.Partition();

        Assertion.All(
                values.TestSequence( expectedValues ),
                errors.TestSequence( expectedErrors ) )
            .Go();
    }

    [Fact]
    public void TryMin_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryMin();
        result.HasValue.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMinData ) )]
    public void TryMin_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
    {
        var result = sut.TryMin();
        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryMax_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryMax();
        result.HasValue.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMaxData ) )]
    public void TryMax_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
    {
        var result = sut.TryMax();
        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryAggregate_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryAggregate( (_, c) => c );
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryAggregate_ShouldReturnWithValue_WhenSourceIsNotEmpty()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected );

        var result = sut.TryAggregate( (_, c) => c );

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryMaxBy_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var result = sut.TryMaxBy( c => c.Value );
        result.HasValue.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMaxData ) )]
    public void TryMaxBy_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.TryMaxBy( c => c.Value );
        result.Value!.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryMinBy_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var result = sut.TryMinBy( c => c.Value );
        result.HasValue.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryMinData ) )]
    public void TryMinBy_ShouldReturnWithValue_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.TryMinBy( c => c.Value );
        result.Value!.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryFirst_ShouldReturnNone_WhenSourceIsEmpty_AndImplementsIReadOnlyList()
    {
        var sut = new List<T>();
        var result = sut.TryFirst();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasOneElement_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = new List<T> { expected };

        var result = sut.TryFirst();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasManyElements_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Prepend( expected ).ToList();

        var result = sut.TryFirst();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryFirst_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryFirst();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasOneElement()
    {
        var expected = Fixture.Create<T>();
        var sut = new[] { expected }.Select( x => x );

        var result = sut.TryFirst();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryFirst_ShouldReturnWithFirstElement_WhenSourceHasManyElements()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Prepend( expected );

        var result = sut.TryFirst();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryLast_ShouldReturnNone_WhenSourceIsEmpty_AndImplementsIReadOnlyList()
    {
        var sut = new List<T>();
        var result = sut.TryLast();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryLast_ShouldReturnWithFirstElement_WhenSourceHasOneElement_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = new List<T> { expected };

        var result = sut.TryLast();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryLast_ShouldReturnWithLastElement_WhenSourceHasManyElements_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected ).ToList();

        var result = sut.TryLast();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryLast_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryLast();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryLast_ShouldReturnWithFirstElement_WhenSourceHasOneElement()
    {
        var expected = Fixture.Create<T>();
        var sut = new[] { expected }.Select( x => x );

        var result = sut.TryLast();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryLast_ShouldReturnWithLastElement_WhenSourceHasManyElements()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected );

        var result = sut.TryLast();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceIsEmpty_AndImplementsIReadOnlyList()
    {
        var sut = new List<T>();
        var result = sut.TrySingle();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TrySingle_ShouldReturnWithFirstElement_WhenSourceHasOneElement_AndImplementsIReadOnlyList()
    {
        var expected = Fixture.Create<T>();
        var sut = new List<T> { expected };

        var result = sut.TrySingle();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceHasManyElements_AndImplementsIReadOnlyList()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.TrySingle();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TrySingle();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TrySingle_ShouldReturnWithFirstElement_WhenSourceHasOneElement()
    {
        var expected = Fixture.Create<T>();
        var sut = new[] { expected }.Select( x => x );

        var result = sut.TrySingle();

        result.Value.TestEquals( expected ).Go();
    }

    [Fact]
    public void TrySingle_ShouldReturnNone_WhenSourceHasManyElements()
    {
        var sut = Fixture.CreateMany<T>().Select( x => x );
        var result = sut.TrySingle();
        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryElementAt_ShouldReturnNone_WhenIndexIsNegative()
    {
        var sut = Fixture.CreateMany<T>();
        var result = sut.TryElementAt( -1 );
        result.HasValue.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtWithTooLargeIndexData ) )]
    public void TryElementAt_ShouldReturnNone_WhenIndexIsGreaterThanOrEqualToElementCount_AndSourceImplementsIReadOnlyList(
        IReadOnlyList<T> sut,
        int index)
    {
        var result = sut.TryElementAt( index );
        result.HasValue.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtData ) )]
    public void TryElementAt_ShouldReturnWithCorrectValue_WhenSourceHasElement_AndImplementsIReadOnlyList(
        IReadOnlyList<T> sut,
        int index,
        T expected)
    {
        var result = sut.TryElementAt( index );
        result.Value.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetTryElementAtWithTooLargeIndexData ) )]
    public void TryElementAt_ShouldReturnNone_WhenIndexIsGreaterThanOrEqualToElementCount(
        IEnumerable<T> sut,
        int index)
    {
        sut = sut.Select( x => x );
        var result = sut.TryElementAt( index );
        result.HasValue.TestFalse().Go();
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
        result.Value.TestEquals( expected ).Go();
    }
}
