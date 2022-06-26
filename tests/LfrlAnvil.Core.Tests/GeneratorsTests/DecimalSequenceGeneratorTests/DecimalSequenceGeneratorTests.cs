using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.DecimalSequenceGeneratorTests;

public class DecimalSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<decimal>
{
    protected sealed override Bounds<decimal> GetDefaultBounds()
    {
        return new Bounds<decimal>( decimal.MinValue, decimal.MaxValue );
    }

    protected sealed override decimal GetDefaultStep()
    {
        return 1;
    }

    protected override decimal Negate(decimal a)
    {
        return -a;
    }

    protected sealed override decimal Add(decimal a, decimal b)
    {
        return a + b;
    }

    protected sealed override SequenceGeneratorBase<decimal> Create()
    {
        return new DecimalSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<decimal> Create(decimal start)
    {
        return new DecimalSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<decimal> Create(decimal start, decimal step)
    {
        return new DecimalSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<decimal> Create(Bounds<decimal> bounds)
    {
        return new DecimalSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<decimal> Create(Bounds<decimal> bounds, decimal start)
    {
        return new DecimalSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<decimal> Create(Bounds<decimal> bounds, decimal start, decimal step)
    {
        return new DecimalSequenceGenerator( bounds, start, step );
    }
}
