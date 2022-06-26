namespace LfrlAnvil.Generators;

public class ByteSequenceGenerator : SequenceGeneratorBase<byte>
{
    public ByteSequenceGenerator()
        : this( start: 0 ) { }

    public ByteSequenceGenerator(byte start)
        : this( start, step: 1 ) { }

    public ByteSequenceGenerator(byte start, byte step)
        : this( new Bounds<byte>( byte.MinValue, byte.MaxValue ), start, step ) { }

    public ByteSequenceGenerator(Bounds<byte> bounds)
        : this( bounds, start: bounds.Min ) { }

    public ByteSequenceGenerator(Bounds<byte> bounds, byte start)
        : this( bounds, start, step: 1 ) { }

    public ByteSequenceGenerator(Bounds<byte> bounds, byte start, byte step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0, nameof( step ) );
    }

    protected sealed override byte AddStep(byte value)
    {
        return checked( (byte)(value + Step) );
    }
}
