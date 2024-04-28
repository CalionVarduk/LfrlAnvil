﻿using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="SByte"/> sequence generator of values within specified range.
/// </summary>
public class SbyteSequenceGenerator : SequenceGeneratorBase<sbyte>
{
    /// <summary>
    /// Creates a new <see cref="SbyteSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    public SbyteSequenceGenerator()
        : this( start: 0 ) { }

    /// <summary>
    /// Creates a new <see cref="SbyteSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public SbyteSequenceGenerator(sbyte start, sbyte step = 1)
        : this( new Bounds<sbyte>( sbyte.MinValue, sbyte.MaxValue ), start, step ) { }

    /// <summary>
    /// Creates a new <see cref="SbyteSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public SbyteSequenceGenerator(Bounds<sbyte> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="SbyteSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">When <paramref name="step"/> is equal to <b>0</b>.</exception>
    public SbyteSequenceGenerator(Bounds<sbyte> bounds, sbyte start, sbyte step = 1)
        : base( bounds, start, step )
    {
        Ensure.NotEquals( step, 0 );
    }

    /// <inheritdoc />
    protected sealed override sbyte AddStep(sbyte value)
    {
        return checked( ( sbyte )(value + Step) );
    }
}
