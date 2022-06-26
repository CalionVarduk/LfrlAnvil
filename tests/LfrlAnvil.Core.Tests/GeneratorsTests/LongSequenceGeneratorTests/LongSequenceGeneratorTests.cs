using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.LongSequenceGeneratorTests;

public class LongSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<long>
{
    protected sealed override Bounds<long> GetDefaultBounds()
    {
        return new Bounds<long>( long.MinValue, long.MaxValue );
    }

    protected sealed override long GetDefaultStep()
    {
        return 1;
    }

    protected override long Negate(long a)
    {
        return -a;
    }

    protected sealed override long Add(long a, long b)
    {
        return a + b;
    }

    protected sealed override SequenceGeneratorBase<long> Create()
    {
        return new LongSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<long> Create(long start)
    {
        return new LongSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<long> Create(long start, long step)
    {
        return new LongSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<long> Create(Bounds<long> bounds)
    {
        return new LongSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<long> Create(Bounds<long> bounds, long start)
    {
        return new LongSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<long> Create(Bounds<long> bounds, long start, long step)
    {
        return new LongSequenceGenerator( bounds, start, step );
    }
}
