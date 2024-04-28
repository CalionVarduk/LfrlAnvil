using System;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="Fixed"/> sequence generator of values within specified range.
/// </summary>
public class FixedSequenceGenerator : SequenceGeneratorBase<Fixed>
{
    /// <summary>
    /// Creates a new <see cref="FixedSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="decimals">Precision of generated values. Equal to <b>0</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="decimals"/> is not a valid <see cref="Fixed"/> precision.
    /// </exception>
    public FixedSequenceGenerator(byte decimals = 0)
        : this( start: Fixed.CreateZero( decimals ) ) { }

    /// <summary>
    /// Creates a new <see cref="FixedSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">
    /// Difference between two consecutively generated values. Equal to <b>1</b> with <paramref name="start"/> precision by default.
    /// </param>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public FixedSequenceGenerator(Fixed start, Fixed? step = null)
        : this( new Bounds<Fixed>( Fixed.MinValue, Fixed.MaxValue ), start, step ?? Fixed.Create( 1, start.Precision ) ) { }

    /// <summary>
    /// Creates a new <see cref="FixedSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public FixedSequenceGenerator(Bounds<Fixed> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="FixedSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">
    /// Difference between two consecutively generated values. Equal to <b>1</b> with <paramref name="start"/> precision by default.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public FixedSequenceGenerator(Bounds<Fixed> bounds, Fixed start, Fixed? step = null)
        : base( bounds, start, step ?? Fixed.Create( 1, start.Precision ) )
    {
        Ensure.NotEquals( Step, Fixed.Zero );
    }

    /// <inheritdoc />
    protected sealed override Fixed AddStep(Fixed value)
    {
        return value + Step;
    }
}
