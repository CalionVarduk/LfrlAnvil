using System;
using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Generators;

/// <inheritdoc />
public abstract class SequenceGeneratorBase<T> : ISequenceGenerator<T>
    where T : IComparable<T>
{
    private Next? _next;

    /// <summary>
    /// Creates a new <see cref="SequenceGeneratorBase{T}"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    protected SequenceGeneratorBase(Bounds<T> bounds, T start, T step)
    {
        Ensure.IsInRange( start, bounds.Min, bounds.Max );
        Bounds = bounds;
        Step = step;
        _next = new Next( start );
    }

    /// <inheritdoc />
    public Bounds<T> Bounds { get; }

    /// <inheritdoc />
    public T Step { get; }

    /// <summary>
    /// Resets the generator by explicitly setting the next value to generate.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <see cref="Bounds"/>> do not contain <paramref name="start"/>.</exception>
    public void Reset(T start)
    {
        Ensure.IsInRange( start, Bounds.Min, Bounds.Max );
        _next = new Next( start );
    }

    /// <inheritdoc />
    public T Generate()
    {
        if ( ! TryGenerate( out var result ) )
            throw new ValueGenerationException();

        return result;
    }

    /// <inheritdoc />
    public bool TryGenerate([MaybeNullWhen( false )] out T result)
    {
        if ( _next is not null && Bounds.Contains( _next.Value.Value ) )
        {
            result = _next.Value.Value;

            try
            {
                _next = new Next( AddStep( _next.Value.Value ) );
            }
            catch ( OverflowException )
            {
                _next = null;
            }

            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Adds <see cref="Step"/> to the previously generated value in order to calculate the next value to generate.
    /// </summary>
    /// <param name="value">Previously generated value.</param>
    /// <returns>Next value to generate.</returns>
    /// <exception cref="OverflowException">When an arithmetic overflow occurred.</exception>
    protected abstract T AddStep(T value);

    object IGenerator.Generate()
    {
        return Generate();
    }

    bool IGenerator.TryGenerate(out object? result)
    {
        if ( TryGenerate( out var underlyingResult ) )
        {
            result = underlyingResult;
            return true;
        }

        result = default;
        return false;
    }

    private readonly struct Next
    {
        internal T Value { get; }

        internal Next(T value)
        {
            Value = value;
        }
    }
}
