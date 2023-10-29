using System;
using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Generators;

public abstract class SequenceGeneratorBase<T> : ISequenceGenerator<T>
    where T : IComparable<T>
{
    private Next? _next;

    protected SequenceGeneratorBase(Bounds<T> bounds, T start, T step)
    {
        Ensure.IsInRange( start, bounds.Min, bounds.Max );
        Bounds = bounds;
        Step = step;
        _next = new Next( start );
    }

    public Bounds<T> Bounds { get; }
    public T Step { get; }

    public void Reset(T start)
    {
        Ensure.IsInRange( start, Bounds.Min, Bounds.Max );
        _next = new Next( start );
    }

    public T Generate()
    {
        if ( ! TryGenerate( out var result ) )
            throw new ValueGenerationException();

        return result;
    }

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
