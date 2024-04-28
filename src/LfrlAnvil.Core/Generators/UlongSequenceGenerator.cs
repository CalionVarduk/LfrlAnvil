using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="UInt64"/> sequence generator of values within specified range.
/// </summary>
public class UlongSequenceGenerator : SequenceGeneratorBase<ulong>
{
    /// <summary>
    /// Creates a new <see cref="UlongSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    public UlongSequenceGenerator()
        : this( start: 0 ) { }

    /// <summary>
    /// Creates a new <see cref="UlongSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public UlongSequenceGenerator(ulong start, ulong step = 1)
        : this( new Bounds<ulong>( ulong.MinValue, ulong.MaxValue ), start, step ) { }

    /// <summary>
    /// Creates a new <see cref="UlongSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public UlongSequenceGenerator(Bounds<ulong> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="UlongSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public UlongSequenceGenerator(Bounds<ulong> bounds, ulong start, ulong step = 1)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0U );
    }

    /// <inheritdoc />
    protected sealed override ulong AddStep(ulong value)
    {
        return checked( value + Step );
    }
}
