namespace LfrlAnvil.Generators;

public class DecimalSequenceGenerator : SequenceGeneratorBase<decimal>
{
    public DecimalSequenceGenerator()
        : this( start: 0 ) { }

    public DecimalSequenceGenerator(decimal start)
        : this( start, step: 1 ) { }

    public DecimalSequenceGenerator(decimal start, decimal step)
        : this( new Bounds<decimal>( decimal.MinValue, decimal.MaxValue ), start, step ) { }

    public DecimalSequenceGenerator(Bounds<decimal> bounds)
        : this( bounds, start: bounds.Min ) { }

    public DecimalSequenceGenerator(Bounds<decimal> bounds, decimal start)
        : this( bounds, start, step: 1 ) { }

    public DecimalSequenceGenerator(Bounds<decimal> bounds, decimal start, decimal step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0, nameof( step ) );
    }

    protected sealed override decimal AddStep(decimal value)
    {
        return value + Step;
    }
}
