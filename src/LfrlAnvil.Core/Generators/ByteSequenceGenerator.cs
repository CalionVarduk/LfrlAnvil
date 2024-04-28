using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="Byte"/> sequence generator of values within specified range.
/// </summary>
public class ByteSequenceGenerator : SequenceGeneratorBase<byte>
{
    /// <summary>
    /// Creates a new <see cref="ByteSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    public ByteSequenceGenerator()
        : this( start: 0 ) { }

    /// <summary>
    /// Creates a new <see cref="ByteSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public ByteSequenceGenerator(byte start, byte step = 1)
        : this( new Bounds<byte>( byte.MinValue, byte.MaxValue ), start, step ) { }

    /// <summary>
    /// Creates a new <see cref="ByteSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public ByteSequenceGenerator(Bounds<byte> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="ByteSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public ByteSequenceGenerator(Bounds<byte> bounds, byte start, byte step = 1)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0 );
    }

    /// <inheritdoc />
    protected sealed override byte AddStep(byte value)
    {
        return checked( ( byte )(value + Step) );
    }
}
