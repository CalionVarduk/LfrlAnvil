using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.UlongSequenceGeneratorTests;

public class UlongSequenceGeneratorTests : GenericSequenceGeneratorTestsBase<ulong>
{
    protected sealed override Bounds<ulong> GetDefaultBounds()
    {
        return new Bounds<ulong>( ulong.MinValue, ulong.MaxValue );
    }

    protected sealed override ulong GetDefaultStep()
    {
        return 1;
    }

    protected sealed override ulong Add(ulong a, ulong b)
    {
        return a + b;
    }

    protected sealed override SequenceGeneratorBase<ulong> Create()
    {
        return new UlongSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<ulong> Create(ulong start)
    {
        return new UlongSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<ulong> Create(ulong start, ulong step)
    {
        return new UlongSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<ulong> Create(Bounds<ulong> bounds)
    {
        return new UlongSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<ulong> Create(Bounds<ulong> bounds, ulong start)
    {
        return new UlongSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<ulong> Create(Bounds<ulong> bounds, ulong start, ulong step)
    {
        return new UlongSequenceGenerator( bounds, start, step );
    }
}
