using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation;

/// <summary>
/// Creates instances of <see cref="IValidator{T,TResult}"/> type.
/// </summary>
/// <typeparam name="TResult">Validator's result type.</typeparam>
public static class Validators<TResult>
{
    /// <summary>
    /// Creates a new <see cref="PassingValidator{T,TResult}"/> instance.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="PassingValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Pass<T>()
    {
        return new PassingValidator<T, TResult>();
    }

    /// <summary>
    /// Creates a new <see cref="FailingValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="FailingValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Fail<T>(TResult failureResult)
    {
        return new FailingValidator<T, TResult>( failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="AndCompositeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validators">Underlying validators.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="AndCompositeValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> All<T>(params IValidator<T, TResult>[] validators)
    {
        return new AndCompositeValidator<T, TResult>( validators );
    }

    /// <summary>
    /// Creates a new <see cref="OrCompositeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validators">Underlying validators.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="OrCompositeValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, OrValidatorResult<TResult>> Any<T>(params IValidator<T, TResult>[] validators)
    {
        return new OrCompositeValidator<T, TResult>( validators );
    }

    /// <summary>
    /// Creates a new <see cref="ConditionalValidator{T,TResult}"/> instance
    /// with a passing <see cref="ConditionalValidator{T,TResult}.IfFalse"/> validator.
    /// </summary>
    /// <param name="condition">
    /// Validator's condition. When it returns <b>true</b>, then <see cref="ConditionalValidator{T,TResult}.IfTrue"/> gets invoked,
    /// otherwise <see cref="ConditionalValidator{T,TResult}.IfFalse"/> gets invoked.
    /// </param>
    /// <param name="validator">
    /// Underlying validator invoked when <see cref="ConditionalValidator{T,TResult}.Condition"/> returns <b>true</b>.
    /// </param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="ConditionalValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfTrue<T>(Func<T, bool> condition, IValidator<T, TResult> validator)
    {
        return Conditional( condition, validator, Pass<T>() );
    }

    /// <summary>
    /// Creates a new <see cref="ConditionalValidator{T,TResult}"/> instance
    /// with a passing <see cref="ConditionalValidator{T,TResult}.IfTrue"/> validator.
    /// </summary>
    /// <param name="condition">
    /// Validator's condition. When it returns <b>true</b>, then <see cref="ConditionalValidator{T,TResult}.IfTrue"/> gets invoked,
    /// otherwise <see cref="ConditionalValidator{T,TResult}.IfFalse"/> gets invoked.
    /// </param>
    /// <param name="validator">
    /// Underlying validator invoked when <see cref="ConditionalValidator{T,TResult}.Condition"/> returns <b>false</b>.
    /// </param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="ConditionalValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfFalse<T>(Func<T, bool> condition, IValidator<T, TResult> validator)
    {
        return Conditional( condition, Pass<T>(), validator );
    }

    /// <summary>
    /// Creates a new <see cref="ConditionalValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="condition">
    /// Validator's condition. When it returns <b>true</b>, then <see cref="ConditionalValidator{T,TResult}.IfTrue"/> gets invoked,
    /// otherwise <see cref="ConditionalValidator{T,TResult}.IfFalse"/> gets invoked.
    /// </param>
    /// <param name="ifTrue">
    /// Underlying validator invoked when <see cref="ConditionalValidator{T,TResult}.Condition"/> returns <b>true</b>.
    /// </param>
    /// <param name="ifFalse">
    /// Underlying validator invoked when <see cref="ConditionalValidator{T,TResult}.Condition"/> returns <b>false</b>.
    /// </param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="ConditionalValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Conditional<T>(
        Func<T, bool> condition,
        IValidator<T, TResult> ifTrue,
        IValidator<T, TResult> ifFalse)
    {
        return new ConditionalValidator<T, TResult>( condition, ifTrue, ifFalse );
    }

    /// <summary>
    /// Creates a new <see cref="SwitchValidator{T,TResult,TSwitchValue}"/> instance
    /// with a passing <see cref="SwitchValidator{T,TResult,TSwitchValue}.DefaultValidator"/> validator.
    /// </summary>
    /// <param name="switchValueSelector">Object's switch value selector.</param>
    /// <param name="validators">Dictionary of validators identified by object's switch values.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TSwitchValue">Object's switch value type used as a validator identifier.</typeparam>
    /// <returns>New <see cref="SwitchValidator{T,TResult,TSwitchValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Switch<T, TSwitchValue>(
        Func<T, TSwitchValue> switchValueSelector,
        IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> validators)
        where TSwitchValue : notnull
    {
        return Switch( switchValueSelector, validators, Pass<T>() );
    }

    /// <summary>
    /// Creates a new <see cref="SwitchValidator{T,TResult,TSwitchValue}"/> instance.
    /// </summary>
    /// <param name="switchValueSelector">Object's switch value selector.</param>
    /// <param name="validators">Dictionary of validators identified by object's switch values.</param>
    /// <param name="defaultValidator">
    /// Default validator that gets chosen when object's switch value does not exist in the
    /// <see cref="SwitchValidator{T,TResult,TSwitchValue}.Validators"/> dictionary.
    /// </param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TSwitchValue">Object's switch value type used as a validator identifier.</typeparam>
    /// <returns>New <see cref="SwitchValidator{T,TResult,TSwitchValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Switch<T, TSwitchValue>(
        Func<T, TSwitchValue> switchValueSelector,
        IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> validators,
        IValidator<T, TResult> defaultValidator)
        where TSwitchValue : notnull
    {
        return new SwitchValidator<T, TResult, TSwitchValue>( switchValueSelector, validators, defaultValidator );
    }

    /// <summary>
    /// Creates a new <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance
    /// with a passing <see cref="TypeCastValidator{T,TDestination,TResult}.IfIsNotOfType"/> validator.
    /// </summary>
    /// <param name="validator">Underlying validator invoked when validated object is of <typeparamref name="TDestination"/> type.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TDestination">
    /// Object type required for <see cref="TypeCastValidator{T,TDestination,TResult}.IfIsOfType"/> validator invocation.
    /// </typeparam>
    /// <returns>New <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfIsOfType<T, TDestination>(IValidator<TDestination, TResult> validator)
    {
        return TypeCast( validator, Pass<T>() );
    }

    /// <summary>
    /// Creates a new <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance
    /// with a passing <see cref="TypeCastValidator{T,TDestination,TResult}.IfIsOfType"/> validator.
    /// </summary>
    /// <param name="validator">
    /// Underlying validator invoked when validated object is not of <typeparamref name="TDestination"/> type.
    /// </param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TDestination">
    /// Object type required for <see cref="TypeCastValidator{T,TDestination,TResult}.IfIsOfType"/> validator invocation.
    /// </typeparam>
    /// <returns>New <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfIsNotOfType<T, TDestination>(IValidator<T, TResult> validator)
    {
        return TypeCast( Pass<TDestination>(), validator );
    }

    /// <summary>
    /// Creates a new <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance.
    /// </summary>
    /// <param name="ifIsOfType">Underlying validator invoked when validated object is of <typeparamref name="TDestination"/> type.</param>
    /// <param name="ifIsNotOfType">
    /// Underlying validator invoked when validated object is not of <typeparamref name="TDestination"/> type.
    /// </param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TDestination">
    /// Object type required for <see cref="TypeCastValidator{T,TDestination,TResult}.IfIsOfType"/> validator invocation.
    /// </typeparam>
    /// <returns>New <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> TypeCast<T, TDestination>(
        IValidator<TDestination, TResult> ifIsOfType,
        IValidator<T, TResult> ifIsNotOfType)
    {
        return new TypeCastValidator<T, TDestination, TResult>( ifIsOfType, ifIsNotOfType );
    }

    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Expected minimum number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MinElementCountValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minCount"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> MinElementCount<T>(int minCount, TResult failureResult)
    {
        return new MinElementCountValidator<T, TResult>( minCount, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="MaxElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MaxElementCountValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxCount"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> MaxElementCount<T>(int maxCount, TResult failureResult)
    {
        return new MaxElementCountValidator<T, TResult>( maxCount, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="count">Expected exact number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountExactValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> ExactElementCount<T>(int count, TResult failureResult)
    {
        return new IsElementCountExactValidator<T, TResult>( count, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minCount"/> is not in [<b>0</b>, <paramref name="maxCount"/>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> ElementCountInRange<T>(int minCount, int maxCount, TResult failureResult)
    {
        return new IsElementCountInRangeValidator<T, TResult>( minCount, maxCount, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance
    /// with <see cref="IsElementCountExactValidator{T,TResult}.Count"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountExactValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> Empty<T>(TResult failureResult)
    {
        return ExactElementCount<T>( count: 0, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance
    /// with <see cref="MinElementCountValidator{T,TResult}.MinCount"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MinElementCountValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> NotEmpty<T>(TResult failureResult)
    {
        return MinElementCount<T>( minCount: 1, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Expected minimum <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="MinLengthValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minLength"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> MinLength(int minLength, TResult failureResult)
    {
        return new MinLengthValidator<TResult>( minLength, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="MaxLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="maxLength">Expected maximum <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="MaxLengthValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxLength"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> MaxLength(int maxLength, TResult failureResult)
    {
        return new MaxLengthValidator<TResult>( maxLength, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="length">Expected exact <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsLengthExactValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> ExactLength(int length, TResult failureResult)
    {
        return new IsLengthExactValidator<TResult>( length, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthInRangeValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Minimum expected <see cref="String.Length"/>.</param>
    /// <param name="maxLength">Maximum expected <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsLengthInRangeValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minLength"/> is not in [<b>0</b>, <paramref name="maxLength"/>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> LengthInRange(int minLength, int maxLength, TResult failureResult)
    {
        return new IsLengthInRangeValidator<TResult>( minLength, maxLength, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance
    /// with <see cref="IsLengthExactValidator{TResult}.Length"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsLengthExactValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> Empty(TResult failureResult)
    {
        return ExactLength( length: 0, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance
    /// with <see cref="MinLengthValidator{TResult}.MinLength"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="MinLengthValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotEmpty(TResult failureResult)
    {
        return MinLength( minLength: 1, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotWhiteSpace(TResult failureResult)
    {
        return new IsNotWhiteSpaceValidator<TResult>( failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotMultilineValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsNotMultilineValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotMultiline(TResult failureResult)
    {
        return new IsNotMultilineValidator<TResult>( failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsRegexMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to match.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsRegexMatchedValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> Match(Regex regex, TResult failureResult)
    {
        return new IsRegexMatchedValidator<TResult>( regex, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsRegexNotMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to not match.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <returns>New <see cref="IsRegexNotMatchedValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotMatch(Regex regex, TResult failureResult)
    {
        return new IsRegexNotMatchedValidator<TResult>( regex, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNullValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Null<T>(TResult failureResult)
    {
        return new IsNullValidator<T, TResult>( failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotNullValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotNull<T>(TResult failureResult)
    {
        return new IsNotNullValidator<T, TResult>( failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsEqualToValidator{T,TResult}"/> instance with <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> EqualTo<T>(T determinant, TResult failureResult)
    {
        return EqualTo( determinant, EqualityComparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> EqualTo<T>(T determinant, IEqualityComparer<T> comparer, TResult failureResult)
    {
        return new IsEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotEqualTo<T>(T determinant, TResult failureResult)
    {
        return NotEqualTo( determinant, EqualityComparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotEqualTo<T>(T determinant, IEqualityComparer<T> comparer, TResult failureResult)
    {
        return new IsNotEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThan<T>(T determinant, TResult failureResult)
    {
        return LessThan( determinant, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThan<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsLessThanValidator<T, TResult>( determinant, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThanOrEqualTo<T>(T determinant, TResult failureResult)
    {
        return LessThanOrEqualTo( determinant, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThanOrEqualTo<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsLessThanOrEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThan<T>(T determinant, TResult failureResult)
    {
        return GreaterThan( determinant, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThan<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsGreaterThanValidator<T, TResult>( determinant, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThanOrEqualTo<T>(T determinant, TResult failureResult)
    {
        return GreaterThanOrEqualTo( determinant, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThanOrEqualTo<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsGreaterThanOrEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsInRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InRange<T>(T min, T max, TResult failureResult)
    {
        return InRange( min, max, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsInRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InExclusiveRange<T>(T min, T max, TResult failureResult)
    {
        return InExclusiveRange( min, max, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InExclusiveRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsInExclusiveRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInRange<T>(T min, T max, TResult failureResult)
    {
        return NotInRange( min, max, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsNotInRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInExclusiveRange<T>(T min, T max, TResult failureResult)
    {
        return NotInExclusiveRange( min, max, Comparer<T>.Default, failureResult );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInExclusiveRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsNotInExclusiveRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }
}
