namespace LfrlAnvil.Generators;

public class ShortSequenceGenerator : SequenceGeneratorBase<short>
{
    public ShortSequenceGenerator()
        : this( start: 0 ) { }

    public ShortSequenceGenerator(short start)
        : this( start, step: 1 ) { }

    public ShortSequenceGenerator(short start, short step)
        : this( new Bounds<short>( short.MinValue, short.MaxValue ), start, step ) { }

    public ShortSequenceGenerator(Bounds<short> bounds)
        : this( bounds, start: bounds.Min ) { }

    public ShortSequenceGenerator(Bounds<short> bounds, short start)
        : this( bounds, start, step: 1 ) { }

    public ShortSequenceGenerator(Bounds<short> bounds, short start, short step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0 );
    }

    protected sealed override short AddStep(short value)
    {
        return checked( (short)(value + Step) );
    }
}
