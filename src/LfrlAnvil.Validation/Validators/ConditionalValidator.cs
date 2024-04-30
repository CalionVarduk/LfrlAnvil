using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a conditional generic object validator.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class ConditionalValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="ConditionalValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="condition">
    /// Validator's condition. When it returns <b>true</b>, then <see cref="IfTrue"/> gets invoked,
    /// otherwise <see cref="IfFalse"/> gets invoked.
    /// </param>
    /// <param name="ifTrue">Underlying validator invoked when <see cref="Condition"/> returns <b>true</b>.</param>
    /// <param name="ifFalse">Underlying validator invoked when <see cref="Condition"/> returns <b>false</b>.</param>
    public ConditionalValidator(Func<T, bool> condition, IValidator<T, TResult> ifTrue, IValidator<T, TResult> ifFalse)
    {
        Condition = condition;
        IfTrue = ifTrue;
        IfFalse = ifFalse;
    }

    /// <summary>
    /// Validator's condition. When it returns <b>true</b>, then <see cref="IfTrue"/> gets invoked,
    /// otherwise <see cref="IfFalse"/> gets invoked.
    /// </summary>
    public Func<T, bool> Condition { get; }

    /// <summary>
    /// Underlying validator invoked when <see cref="Condition"/> returns <b>true</b>.
    /// </summary>
    public IValidator<T, TResult> IfTrue { get; }

    /// <summary>
    /// Underlying validator invoked when <see cref="Condition"/> returns <b>false</b>.
    /// </summary>
    public IValidator<T, TResult> IfFalse { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var validator = Condition( obj ) ? IfTrue : IfFalse;
        return validator.Validate( obj );
    }
}
