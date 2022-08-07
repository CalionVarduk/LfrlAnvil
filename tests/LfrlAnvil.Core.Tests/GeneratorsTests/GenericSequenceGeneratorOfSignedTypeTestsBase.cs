using FluentAssertions.Execution;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.GeneratorsTests;

public abstract class GenericSequenceGeneratorOfSignedTypeTestsBase<T> : GenericSequenceGeneratorTestsBase<T>
    where T : struct, IComparable<T>
{
    [Fact]
    public void Generate_ShouldThrowValueGenerationException_WhenNextValueIsLessThanMin()
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        var step = Negate( GetDefaultStep() );
        var sut = Create( Bounds.Create( min, max ), min, step );
        sut.Generate();

        var action = Lambda.Of( () => sut.Generate() );

        action.Should().ThrowExactly<ValueGenerationException>();
    }

    [Fact]
    public void Generate_ShouldThrowValueGenerationException_WhenNextValueCausesNegativeArithmeticOverflow()
    {
        var step = Negate( GetDefaultStep() );
        var sut = Create( default( T ), step );
        sut.Reset( sut.Bounds.Min );
        sut.Generate();

        var action = Lambda.Of( () => sut.Generate() );

        action.Should().ThrowExactly<ValueGenerationException>();
    }

    [Fact]
    public void TryGenerate_ShouldReturnFalse_WhenNextValueIsLessThanMin()
    {
        var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        var step = Negate( GetDefaultStep() );
        var sut = Create( Bounds.Create( min, max ), min, step );
        sut.Generate();

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void TryGenerate_ShouldReturnFalse_WhenNextValueCausesNegativeArithmeticOverflow()
    {
        var step = Negate( GetDefaultStep() );
        var sut = Create( default( T ), step );
        sut.Reset( sut.Bounds.Min );
        sut.Generate();

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    protected abstract T Negate(T a);
}
