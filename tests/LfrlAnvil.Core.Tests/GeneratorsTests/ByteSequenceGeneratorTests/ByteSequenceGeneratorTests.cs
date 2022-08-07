using LfrlAnvil.Generators;
using ByteSequenceGenerator = LfrlAnvil.Generators.ByteSequenceGenerator;

namespace LfrlAnvil.Tests.GeneratorsTests.ByteSequenceGeneratorTests;

public class ByteSequenceGeneratorTests : GenericSequenceGeneratorTestsBase<byte>
{
    protected sealed override Bounds<byte> GetDefaultBounds()
    {
        return new Bounds<byte>( byte.MinValue, byte.MaxValue );
    }

    protected sealed override byte GetDefaultStep()
    {
        return 1;
    }

    protected sealed override byte Add(byte a, byte b)
    {
        return (byte)(a + b);
    }

    protected sealed override SequenceGeneratorBase<byte> Create()
    {
        return new ByteSequenceGenerator();
    }

    protected sealed override SequenceGeneratorBase<byte> Create(byte start)
    {
        return new ByteSequenceGenerator( start );
    }

    protected sealed override SequenceGeneratorBase<byte> Create(byte start, byte step)
    {
        return new ByteSequenceGenerator( start, step );
    }

    protected sealed override SequenceGeneratorBase<byte> Create(Bounds<byte> bounds)
    {
        return new ByteSequenceGenerator( bounds );
    }

    protected sealed override SequenceGeneratorBase<byte> Create(Bounds<byte> bounds, byte start)
    {
        return new ByteSequenceGenerator( bounds, start );
    }

    protected sealed override SequenceGeneratorBase<byte> Create(Bounds<byte> bounds, byte start, byte step)
    {
        return new ByteSequenceGenerator( bounds, start, step );
    }
}
