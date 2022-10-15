using LfrlAnvil.Numerics;

namespace LfrlAnvil.Generators;

public class FixedSequenceGenerator : SequenceGeneratorBase<Fixed>
{
    public FixedSequenceGenerator(byte decimals = 0)
        : this( start: Fixed.CreateZero( decimals ) ) { }

    public FixedSequenceGenerator(Fixed start)
        : this( start, step: Fixed.Create( 1, start.Precision ) ) { }

    public FixedSequenceGenerator(Fixed start, Fixed step)
        : this( new Bounds<Fixed>( Fixed.MinValue, Fixed.MaxValue ), start, step ) { }

    public FixedSequenceGenerator(Bounds<Fixed> bounds)
        : this( bounds, start: bounds.Min ) { }

    public FixedSequenceGenerator(Bounds<Fixed> bounds, Fixed start)
        : this( bounds, start, step: Fixed.Create( 1, start.Precision ) ) { }

    public FixedSequenceGenerator(Bounds<Fixed> bounds, Fixed start, Fixed step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, Fixed.Zero, nameof( step ) );
    }

    protected sealed override Fixed AddStep(Fixed value)
    {
        return value + Step;
    }
}
