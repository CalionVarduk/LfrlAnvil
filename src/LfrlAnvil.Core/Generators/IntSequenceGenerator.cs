namespace LfrlAnvil.Generators;

public class IntSequenceGenerator : SequenceGeneratorBase<int>
{
    public IntSequenceGenerator()
        : this( start: 0 ) { }

    public IntSequenceGenerator(int start)
        : this( start, step: 1 ) { }

    public IntSequenceGenerator(int start, int step)
        : this( new Bounds<int>( int.MinValue, int.MaxValue ), start, step ) { }

    public IntSequenceGenerator(Bounds<int> bounds)
        : this( bounds, start: bounds.Min ) { }

    public IntSequenceGenerator(Bounds<int> bounds, int start)
        : this( bounds, start, step: 1 ) { }

    public IntSequenceGenerator(Bounds<int> bounds, int start, int step)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0 );
    }

    protected sealed override int AddStep(int value)
    {
        return checked( value + Step );
    }
}
