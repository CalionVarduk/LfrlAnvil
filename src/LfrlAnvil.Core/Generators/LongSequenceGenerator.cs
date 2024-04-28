using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="Int64"/> sequence generator of values within specified range.
/// </summary>
public class LongSequenceGenerator : SequenceGeneratorBase<long>
{
    /// <summary>
    /// Creates a new <see cref="LongSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    public LongSequenceGenerator()
        : this( start: 0 ) { }

    /// <summary>
    /// Creates a new <see cref="LongSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public LongSequenceGenerator(long start, long step = 1)
        : this( new Bounds<long>( long.MinValue, long.MaxValue ), start, step ) { }

    /// <summary>
    /// Creates a new <see cref="LongSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public LongSequenceGenerator(Bounds<long> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="LongSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public LongSequenceGenerator(Bounds<long> bounds, long start, long step = 1)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0 );
    }

    /// <inheritdoc />
    protected sealed override long AddStep(long value)
    {
        return checked( value + Step );
    }
}
