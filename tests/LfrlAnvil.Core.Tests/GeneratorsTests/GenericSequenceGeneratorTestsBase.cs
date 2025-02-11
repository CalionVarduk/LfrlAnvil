using LfrlAnvil.Exceptions;
using LfrlAnvil.Functional;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests;

public abstract class GenericSequenceGeneratorTestsBase<T> : TestsBase
    where T : struct, IComparable<T>
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var expectedBounds = GetDefaultBounds();
        var expectedStep = GetDefaultStep();
        var expectedStart = Fixture.CreateDefault<T>();

        var sut = Create();
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( expectedStart ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithStart_ShouldReturnCorrectResult()
    {
        var expectedBounds = GetDefaultBounds();
        var expectedStep = GetDefaultStep();
        var expectedStart = Fixture.Create<T>();

        var sut = Create( expectedStart );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( expectedStart ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithStartAndStep_ShouldReturnCorrectResult()
    {
        var expectedBounds = GetDefaultBounds();
        var expectedStep = Fixture.CreateNotDefault<T>();
        var expectedStart = Fixture.Create<T>();

        var sut = Create( expectedStart, expectedStep );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( expectedStart ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithStartAndStep_ShouldThrowArgumentException_WhenStepIsInvalid()
    {
        var expectedStep = Fixture.CreateDefault<T>();
        var expectedStart = Fixture.Create<T>();

        var action = Lambda.Of( () => Create( expectedStart, expectedStep ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Ctor_WithBounds_ShouldReturnCorrectResult()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = GetDefaultStep();

        var sut = Create( expectedBounds );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( expectedBounds.Min ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStart_ShouldReturnCorrectResult_WhenStartIsExclusivelyContainedInBounds()
    {
        var (min, start, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = GetDefaultStep();

        var sut = Create( expectedBounds, start );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( start ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStart_ShouldReturnCorrectResult_WhenStartIsEqualToMin()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = GetDefaultStep();

        var sut = Create( expectedBounds, min );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( min ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStart_ShouldReturnCorrectResult_WhenStartIsEqualToMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = GetDefaultStep();

        var sut = Create( expectedBounds, max );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( max ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStart_ShouldThrowArgumentOutOfRangeException_WhenStartIsLessThanMin()
    {
        var (start, min, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );

        var action = Lambda.Of( () => Create( expectedBounds, start ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStart_ShouldThrowArgumentOutOfRangeException_WhenStartIsGreaterThanMax()
    {
        var (min, max, start) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );

        var action = Lambda.Of( () => Create( expectedBounds, start ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStartAndStep_ShouldReturnCorrectResult_WhenStartIsExclusivelyContainedInBounds()
    {
        var (min, start, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = Fixture.CreateNotDefault<T>();

        var sut = Create( expectedBounds, start, expectedStep );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( start ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStartAndStep_ShouldReturnCorrectResult_WhenStartIsEqualToMin()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = Fixture.CreateNotDefault<T>();

        var sut = Create( expectedBounds, min, expectedStep );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( min ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStartAndStep_ShouldReturnCorrectResult_WhenStartIsEqualToMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = Fixture.CreateNotDefault<T>();

        var sut = Create( expectedBounds, max, expectedStep );
        var firstValue = sut.Generate();

        Assertion.All(
                sut.Bounds.TestEquals( expectedBounds ),
                sut.Step.TestEquals( expectedStep ),
                firstValue.TestEquals( max ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentOutOfRangeException_WhenStartIsLessThanMin()
    {
        var (start, min, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = Fixture.CreateNotDefault<T>();

        var action = Lambda.Of( () => Create( expectedBounds, start, expectedStep ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentOutOfRangeException_WhenStartIsGreaterThanMax()
    {
        var (min, max, start) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = Fixture.CreateNotDefault<T>();

        var action = Lambda.Of( () => Create( expectedBounds, start, expectedStep ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenStepIsInvalid()
    {
        var (min, start, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var expectedBounds = Bounds.Create( min, max );
        var expectedStep = Fixture.CreateDefault<T>();

        var action = Lambda.Of( () => Create( expectedBounds, start, expectedStep ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Reset_ShouldChangeNextGeneratedValue_WhenStartIsExclusivelyContainedInBounds()
    {
        var (min, start, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var sut = Create( Bounds.Create( min, max ) );

        sut.Reset( start );
        var firstValue = sut.Generate();

        firstValue.TestEquals( start ).Go();
    }

    [Fact]
    public void Reset_ShouldChangeNextGeneratedValue_WhenStartIsEqualToMin()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var sut = Create( Bounds.Create( min, max ), max );

        sut.Reset( min );
        var firstValue = sut.Generate();

        firstValue.TestEquals( min ).Go();
    }

    [Fact]
    public void Reset_ShouldChangeNextGeneratedValue_WhenStartIsEqualToMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var sut = Create( Bounds.Create( min, max ) );

        sut.Reset( max );
        var firstValue = sut.Generate();

        firstValue.TestEquals( max ).Go();
    }

    [Fact]
    public void Reset_ShouldThrowArgumentOutOfRangeException_WhenStartIsLessThanMin()
    {
        var (start, min, max) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var sut = Create( Bounds.Create( min, max ) );

        var action = Lambda.Of( () => sut.Reset( start ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Reset_ShouldThrowArgumentOutOfRangeException_WhenStartIsGreaterThanMax()
    {
        var (min, max, start) = Fixture.CreateManyDistinctSorted<T>( count: 3 );
        var sut = Create( Bounds.Create( min, max ) );

        var action = Lambda.Of( () => sut.Reset( start ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Generate_ShouldReturnCorrectValue_WhenNextValueIsInBounds()
    {
        var start = Fixture.CreateDefault<T>();
        var step = Fixture.CreateNotDefault<T>();
        var sut = Create( start, step );
        var expected = Add( start, step );
        sut.Generate();

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Generate_ShouldThrowValueGenerationException_WhenNextValueIsGreaterThanMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var step = GetDefaultStep();
        var sut = Create( Bounds.Create( min, max ), max, step );
        sut.Generate();

        var action = Lambda.Of( () => sut.Generate() );

        action.Test( exc => exc.TestType().Exact<ValueGenerationException>() ).Go();
    }

    [Fact]
    public void Generate_ShouldThrowValueGenerationException_WhenNextValueCausesArithmeticOverflow()
    {
        var sut = Create();
        sut.Reset( sut.Bounds.Max );
        sut.Generate();

        var action = Lambda.Of( () => sut.Generate() );

        action.Test( exc => exc.TestType().Exact<ValueGenerationException>() ).Go();
    }

    [Fact]
    public void TryGenerate_ShouldReturnTrueAndCorrectValue_WhenNextValueIsInBounds()
    {
        var start = Fixture.CreateDefault<T>();
        var step = Fixture.CreateNotDefault<T>();
        var sut = Create( start, step );
        var expected = Add( start, step );
        sut.Generate();

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void TryGenerate_ShouldReturnFalse_WhenNextValueIsGreaterThanMax()
    {
        var (min, max) = Fixture.CreateManyDistinctSorted<T>( count: 2 );
        var step = GetDefaultStep();
        var sut = Create( Bounds.Create( min, max ), max, step );
        sut.Generate();

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryGenerate_ShouldReturnFalse_WhenNextValueCausesArithmeticOverflow()
    {
        var sut = Create();
        sut.Reset( sut.Bounds.Max );
        sut.Generate();

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void IGeneratorGenerate_ShouldBeEquivalentToGenerate()
    {
        var other = Create();
        IGenerator sut = Create();
        var expected = other.Generate();

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IGeneratorTryGenerate_ShouldBeEquivalentToTryGenerate_WhenNextValueIsInBounds()
    {
        var other = Create();
        IGenerator sut = Create();
        var expected = other.TryGenerate( out var outExpected );

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestEquals( outExpected ) )
            .Go();
    }

    [Fact]
    public void IGeneratorTryGenerate_ShouldBeEquivalentToTryGenerate_WhenNextValueIsOutOfBounds()
    {
        var other = Create();
        other.Reset( other.Bounds.Max );
        other.Generate();

        var sut = Create();
        sut.Reset( sut.Bounds.Max );
        sut.Generate();

        var expected = other.TryGenerate( out _ );

        var result = (( IGenerator )sut).TryGenerate( out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestNull() )
            .Go();
    }

    protected abstract Bounds<T> GetDefaultBounds();
    protected abstract T GetDefaultStep();
    protected abstract T Add(T a, T b);

    protected abstract SequenceGeneratorBase<T> Create();
    protected abstract SequenceGeneratorBase<T> Create(T start);
    protected abstract SequenceGeneratorBase<T> Create(T start, T step);
    protected abstract SequenceGeneratorBase<T> Create(Bounds<T> bounds);
    protected abstract SequenceGeneratorBase<T> Create(Bounds<T> bounds, T start);
    protected abstract SequenceGeneratorBase<T> Create(Bounds<T> bounds, T start, T step);
}
