using LfrlAnvil.Functional;
using LfrlAnvil.Generators;
using DoubleSequenceGenerator = LfrlAnvil.Generators.DoubleSequenceGenerator;

namespace LfrlAnvil.Tests.GeneratorsTests.DoubleSequenceGeneratorTests;

public class DoubleSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<double>
{
    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithStart_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(double start)
    {
        var action = Lambda.Of( () => Create( start ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithStartAndStep_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(double start)
    {
        var action = Lambda.Of( () => Create( start ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithStartAndStep_ShouldThrowArgumentException_WhenStepIsNotFinite(double step)
    {
        var start = Fixture.Create<double>();
        var action = Lambda.Of( () => Create( start, step ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBounds_ShouldThrowArgumentException_WhenMinIsNotFinite(double min)
    {
        var max = Fixture.Create<double>();
        var action = Lambda.Of( () => Create( Bounds.Create( min, max ) ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBounds_ShouldThrowArgumentException_WhenMaxIsNotFinite(double max)
    {
        var min = Fixture.Create<double>();
        var action = Lambda.Of( () => Create( Bounds.Create( min, max ) ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStart_ShouldThrowArgumentException_WhenMinIsNotFinite(double min)
    {
        var (start, max) = Fixture.CreateDistinctSortedCollection<double>( 2 );
        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStart_ShouldThrowArgumentException_WhenMaxIsNotFinite(double max)
    {
        var (min, start) = Fixture.CreateDistinctSortedCollection<double>( 2 );
        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStart_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(double start)
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<double>( 2 );
        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenMinIsNotFinite(double min)
    {
        var (start, max) = Fixture.CreateDistinctSortedCollection<double>( 2 );
        var step = GetDefaultStep();

        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenMaxIsNotFinite(double max)
    {
        var (min, start) = Fixture.CreateDistinctSortedCollection<double>( 2 );
        var step = GetDefaultStep();

        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(double start)
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<double>( 2 );
        var step = GetDefaultStep();

        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.NegativeInfinity )]
    [InlineData( double.PositiveInfinity )]
    public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenStepIsNotFinite(double step)
    {
        var (min, start, max) = Fixture.CreateDistinctSortedCollection<double>( 3 );
        var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    protected sealed override Bounds<double> GetDefaultBounds()
    {
        return new Bounds<double>( double.MinValue, double.MaxValue );
    }

    protected sealed override double GetDefaultStep()
    {
        return 1;
    }

    protected override double Negate(double a)
    {
        return -a;
    }

    protected sealed override double Add(double a, double b)
    {
        return a + b;
    }

    protected sealed override SequenceGeneratorBase<double> Create()
    {
        return new DoubleSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<double> Create(double start)
    {
        return new DoubleSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<double> Create(double start, double step)
    {
        return new DoubleSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<double> Create(Bounds<double> bounds)
    {
        return new DoubleSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<double> Create(Bounds<double> bounds, double start)
    {
        return new DoubleSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<double> Create(Bounds<double> bounds, double start, double step)
    {
        return new DoubleSequenceGenerator( bounds, start, step );
    }
}
