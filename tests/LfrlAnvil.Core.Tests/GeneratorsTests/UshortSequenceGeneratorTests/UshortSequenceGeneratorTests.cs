using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.UshortSequenceGeneratorTests;

public class UshortSequenceGeneratorTests : GenericSequenceGeneratorTestsBase<ushort>
{
    protected sealed override Bounds<ushort> GetDefaultBounds()
    {
        return new Bounds<ushort>( ushort.MinValue, ushort.MaxValue );
    }

    protected sealed override ushort GetDefaultStep()
    {
        return 1;
    }

    protected sealed override ushort Add(ushort a, ushort b)
    {
        return (ushort)(a + b);
    }

    protected sealed override SequenceGeneratorBase<ushort> Create()
    {
        return new UshortSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<ushort> Create(ushort start)
    {
        return new UshortSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<ushort> Create(ushort start, ushort step)
    {
        return new UshortSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<ushort> Create(Bounds<ushort> bounds)
    {
        return new UshortSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<ushort> Create(Bounds<ushort> bounds, ushort start)
    {
        return new UshortSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<ushort> Create(Bounds<ushort> bounds, ushort start, ushort step)
    {
        return new UshortSequenceGenerator( bounds, start, step );
    }
}
