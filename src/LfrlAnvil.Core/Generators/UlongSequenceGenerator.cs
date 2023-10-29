namespace LfrlAnvil.Generators;

public class UlongSequenceGenerator : SequenceGeneratorBase<ulong>
{
    public UlongSequenceGenerator()
        : this( start: 0 ) { }

    public UlongSequenceGenerator(ulong start)
        : this( start, step: 1 ) { }

    public UlongSequenceGenerator(ulong start, ulong step)
        : this( new Bounds<ulong>( ulong.MinValue, ulong.MaxValue ), start, step ) { }

    public UlongSequenceGenerator(Bounds<ulong> bounds)
        : this( bounds, start: bounds.Min ) { }

    public UlongSequenceGenerator(Bounds<ulong> bounds, ulong start)
        : this( bounds, start, step: 1 ) { }

    public UlongSequenceGenerator(Bounds<ulong> bounds, ulong start, ulong step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0U );
    }

    protected sealed override ulong AddStep(ulong value)
    {
        return checked( value + Step );
    }
}
