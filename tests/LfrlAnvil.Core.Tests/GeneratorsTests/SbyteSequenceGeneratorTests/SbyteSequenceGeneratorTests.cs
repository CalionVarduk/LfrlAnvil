using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.SbyteSequenceGeneratorTests;

public class SbyteSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<sbyte>
{
    protected sealed override Bounds<sbyte> GetDefaultBounds()
    {
        return new Bounds<sbyte>( sbyte.MinValue, sbyte.MaxValue );
    }

    protected sealed override sbyte GetDefaultStep()
    {
        return 1;
    }

    protected override sbyte Negate(sbyte a)
    {
        return (sbyte)-a;
    }

    protected sealed override sbyte Add(sbyte a, sbyte b)
    {
        return (sbyte)(a + b);
    }

    protected sealed override SequenceGeneratorBase<sbyte> Create()
    {
        return new SbyteSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<sbyte> Create(sbyte start)
    {
        return new SbyteSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<sbyte> Create(sbyte start, sbyte step)
    {
        return new SbyteSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<sbyte> Create(Bounds<sbyte> bounds)
    {
        return new SbyteSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<sbyte> Create(Bounds<sbyte> bounds, sbyte start)
    {
        return new SbyteSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<sbyte> Create(Bounds<sbyte> bounds, sbyte start, sbyte step)
    {
        return new SbyteSequenceGenerator( bounds, start, step );
    }
}
