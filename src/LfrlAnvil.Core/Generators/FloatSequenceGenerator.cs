using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="Single"/> sequence generator of values within specified range.
/// </summary>
public class FloatSequenceGenerator : SequenceGeneratorBase<float>
{
    /// <summary>
    /// Creates a new <see cref="FloatSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    public FloatSequenceGenerator()
        : this( start: 0 ) { }

    /// <summary>
    /// Creates a new <see cref="FloatSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentException">
    /// When <paramref name="step"/> is equal to <b>0</b> or any of the values is not a finite number.
    /// </exception>
    public FloatSequenceGenerator(float start, float step = 1)
        : this( new Bounds<float>( float.MinValue, float.MaxValue ), start, step ) { }

    /// <summary>
    /// Creates a new <see cref="FloatSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public FloatSequenceGenerator(Bounds<float> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="FloatSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">
    /// When <paramref name="step"/> is equal to <b>0</b> or any of the values is not a finite number.
    /// </exception>
    public FloatSequenceGenerator(Bounds<float> bounds, float start, float step = 1)
        : base( bounds, start, step )
    {
        Ensure.False( float.IsNaN( bounds.Min ) );
        Ensure.NotEquals( bounds.Min, float.NegativeInfinity );
        Ensure.NotEquals( bounds.Min, float.PositiveInfinity );
        Ensure.False( float.IsNaN( bounds.Max ) );
        Ensure.NotEquals( bounds.Max, float.NegativeInfinity );
        Ensure.NotEquals( bounds.Max, float.PositiveInfinity );
        Ensure.False( float.IsNaN( step ) );
        Ensure.NotEquals( step, float.NegativeInfinity );
        Ensure.NotEquals( step, float.PositiveInfinity );
        Ensure.NotEquals( step, 0 );
    }

    /// <inheritdoc />
    protected sealed override float AddStep(float value)
    {
        var result = value + Step;
        if ( result.Equals( value ) )
            throw new OverflowException();

        return result;
    }
}
