using System;

namespace LfrlAnvil.Generators;

public class FloatSequenceGenerator : SequenceGeneratorBase<float>
{
    public FloatSequenceGenerator()
        : this( start: 0 ) { }

    public FloatSequenceGenerator(float start)
        : this( start, step: 1 ) { }

    public FloatSequenceGenerator(float start, float step)
        : this( new Bounds<float>( float.MinValue, float.MaxValue ), start, step ) { }

    public FloatSequenceGenerator(Bounds<float> bounds)
        : this( bounds, start: bounds.Min ) { }

    public FloatSequenceGenerator(Bounds<float> bounds, float start)
        : this( bounds, start, step: 1 ) { }

    public FloatSequenceGenerator(Bounds<float> bounds, float start, float step)
        : base( bounds, start, step )
    {
        Ensure.False( float.IsNaN( bounds.Min ), nameof( bounds ) + "." + nameof( bounds.Min ) + " cannot be NaN" );
        Ensure.NotEquals( bounds.Min, float.NegativeInfinity, nameof( bounds ) + "." + nameof( bounds.Min ) );
        Ensure.NotEquals( bounds.Min, float.PositiveInfinity, nameof( bounds ) + "." + nameof( bounds.Min ) );
        Ensure.False( float.IsNaN( bounds.Max ), nameof( bounds ) + "." + nameof( bounds.Max ) + " cannot be NaN" );
        Ensure.NotEquals( bounds.Max, float.NegativeInfinity, nameof( bounds ) + "." + nameof( bounds.Max ) );
        Ensure.NotEquals( bounds.Max, float.PositiveInfinity, nameof( bounds ) + "." + nameof( bounds.Max ) );
        Ensure.False( float.IsNaN( step ), nameof( step ) + " cannot be NaN" );
        Ensure.NotEquals( step, float.NegativeInfinity, nameof( step ) );
        Ensure.NotEquals( step, float.PositiveInfinity, nameof( step ) );
        Ensure.NotEquals( step, 0, nameof( step ) );
    }

    protected sealed override float AddStep(float value)
    {
        var result = value + Step;
        if ( result.Equals( value ) )
            throw new OverflowException();

        return result;
    }
}
