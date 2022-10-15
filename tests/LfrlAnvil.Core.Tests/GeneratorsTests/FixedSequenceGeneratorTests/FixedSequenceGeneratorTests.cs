using LfrlAnvil.Generators;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Tests.GeneratorsTests.FixedSequenceGeneratorTests;

public class FixedSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<Fixed>
{
    public FixedSequenceGeneratorTests()
    {
        Fixture.Customize<Fixed>( c => c.FromFactory( () => Fixed.Create( Fixture.Create<long>() ) ) );
    }

    protected sealed override Bounds<Fixed> GetDefaultBounds()
    {
        return new Bounds<Fixed>( Fixed.MinValue, Fixed.MaxValue );
    }

    protected sealed override Fixed GetDefaultStep()
    {
        return Fixed.Create( 1 );
    }

    protected override Fixed Negate(Fixed a)
    {
        return -a;
    }

    protected sealed override Fixed Add(Fixed a, Fixed b)
    {
        return a + b;
    }

    protected sealed override SequenceGeneratorBase<Fixed> Create()
    {
        return new FixedSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<Fixed> Create(Fixed start)
    {
        return new FixedSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<Fixed> Create(Fixed start, Fixed step)
    {
        return new FixedSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<Fixed> Create(Bounds<Fixed> bounds)
    {
        return new FixedSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<Fixed> Create(Bounds<Fixed> bounds, Fixed start)
    {
        return new FixedSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<Fixed> Create(Bounds<Fixed> bounds, Fixed start, Fixed step)
    {
        return new FixedSequenceGenerator( bounds, start, step );
    }
}
