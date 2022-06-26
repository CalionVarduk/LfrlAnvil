namespace LfrlAnvil.Generators;

public class UshortSequenceGenerator : SequenceGeneratorBase<ushort>
{
    public UshortSequenceGenerator()
        : this( start: 0 ) { }

    public UshortSequenceGenerator(ushort start)
        : this( start, step: 1 ) { }

    public UshortSequenceGenerator(ushort start, ushort step)
        : this( new Bounds<ushort>( ushort.MinValue, ushort.MaxValue ), start, step ) { }

    public UshortSequenceGenerator(Bounds<ushort> bounds)
        : this( bounds, start: bounds.Min ) { }

    public UshortSequenceGenerator(Bounds<ushort> bounds, ushort start)
        : this( bounds, start, step: 1 ) { }

    public UshortSequenceGenerator(Bounds<ushort> bounds, ushort start, ushort step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0U, nameof( step ) );
    }

    protected sealed override ushort AddStep(ushort value)
    {
        return checked( (ushort)(value + Step) );
    }
}
