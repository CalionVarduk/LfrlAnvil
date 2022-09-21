using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation;

public static class Validators<TResult>
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Pass<T>()
    {
        return new PassingValidator<T, TResult>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Fail<T>(TResult failureResult)
    {
        return new FailingValidator<T, TResult>( failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> All<T>(params IValidator<T, TResult>[] validators)
    {
        return new AndCompositeValidator<T, TResult>( validators );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, OrValidatorResult<TResult>> Any<T>(params IValidator<T, TResult>[] validators)
    {
        return new OrCompositeValidator<T, TResult>( validators );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfTrue<T>(Func<T, bool> condition, IValidator<T, TResult> validator)
    {
        return Conditional( condition, validator, Pass<T>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfFalse<T>(Func<T, bool> condition, IValidator<T, TResult> validator)
    {
        return Conditional( condition, Pass<T>(), validator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Conditional<T>(
        Func<T, bool> condition,
        IValidator<T, TResult> ifTrue,
        IValidator<T, TResult> ifFalse)
    {
        return new ConditionalValidator<T, TResult>( condition, ifTrue, ifFalse );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Switch<T, TSwitchValue>(
        Func<T, TSwitchValue> switchValueSelector,
        IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> validators)
        where TSwitchValue : notnull
    {
        return Switch( switchValueSelector, validators, Pass<T>() );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfIsOfType<T, TDestination>(IValidator<TDestination, TResult> validator)
    {
        return TypeCast( validator, Pass<T>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> IfIsNotOfType<T, TDestination>(IValidator<T, TResult> validator)
    {
        return TypeCast( Pass<TDestination>(), validator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> TypeCast<T, TDestination>(
        IValidator<TDestination, TResult> ifIsOfType,
        IValidator<T, TResult> ifIsNotOfType)
    {
        return new TypeCastValidator<T, TDestination, TResult>( ifIsOfType, ifIsNotOfType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> MinElementCount<T>(int minCount, TResult failureResult)
    {
        return new MinElementCountValidator<T, TResult>( minCount, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> MaxElementCount<T>(int maxCount, TResult failureResult)
    {
        return new MaxElementCountValidator<T, TResult>( maxCount, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> ExactElementCount<T>(int count, TResult failureResult)
    {
        return new IsElementCountExactValidator<T, TResult>( count, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> ElementCountInRange<T>(int minCount, int maxCount, TResult failureResult)
    {
        return new IsElementCountInRangeValidator<T, TResult>( minCount, maxCount, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> Empty<T>(TResult failureResult)
    {
        return ExactElementCount<T>( count: 0, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, TResult> NotEmpty<T>(TResult failureResult)
    {
        return MinElementCount<T>( minCount: 1, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> MinLength(int minLength, TResult failureResult)
    {
        return new MinLengthValidator<TResult>( minLength, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> MaxLength(int maxLength, TResult failureResult)
    {
        return new MaxLengthValidator<TResult>( maxLength, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> ExactLength(int length, TResult failureResult)
    {
        return new IsLengthExactValidator<TResult>( length, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> LengthInRange(int minLength, int maxLength, TResult failureResult)
    {
        return new IsLengthInRangeValidator<TResult>( minLength, maxLength, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> Empty(TResult failureResult)
    {
        return ExactLength( length: 0, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotEmpty(TResult failureResult)
    {
        return MinLength( minLength: 1, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotWhiteSpace(TResult failureResult)
    {
        return new IsNotWhiteSpaceValidator<TResult>( failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotMultiline(TResult failureResult)
    {
        return new IsNotMultilineValidator<TResult>( failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> Match(Regex regex, TResult failureResult)
    {
        return new IsRegexMatchedValidator<TResult>( regex, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, TResult> NotMatch(Regex regex, TResult failureResult)
    {
        return new IsRegexNotMatchedValidator<TResult>( regex, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Null<T>(TResult failureResult)
    {
        return new IsNullValidator<T, TResult>( failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotNull<T>(TResult failureResult)
    {
        return new IsNotNullValidator<T, TResult>( failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> EqualTo<T>(T determinant, TResult failureResult)
    {
        return EqualTo( determinant, EqualityComparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> EqualTo<T>(T determinant, IEqualityComparer<T> comparer, TResult failureResult)
    {
        return new IsEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotEqualTo<T>(T determinant, TResult failureResult)
    {
        return NotEqualTo( determinant, EqualityComparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotEqualTo<T>(T determinant, IEqualityComparer<T> comparer, TResult failureResult)
    {
        return new IsNotEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThan<T>(T determinant, TResult failureResult)
    {
        return LessThan( determinant, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThan<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsLessThanValidator<T, TResult>( determinant, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThanOrEqualTo<T>(T determinant, TResult failureResult)
    {
        return LessThanOrEqualTo( determinant, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> LessThanOrEqualTo<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsLessThanOrEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThan<T>(T determinant, TResult failureResult)
    {
        return GreaterThan( determinant, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThan<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsGreaterThanValidator<T, TResult>( determinant, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThanOrEqualTo<T>(T determinant, TResult failureResult)
    {
        return GreaterThanOrEqualTo( determinant, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> GreaterThanOrEqualTo<T>(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        return new IsGreaterThanOrEqualToValidator<T, TResult>( determinant, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InRange<T>(T min, T max, TResult failureResult)
    {
        return InRange( min, max, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsInRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InExclusiveRange<T>(T min, T max, TResult failureResult)
    {
        return InExclusiveRange( min, max, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> InExclusiveRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsInExclusiveRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInRange<T>(T min, T max, TResult failureResult)
    {
        return NotInRange( min, max, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsNotInRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInExclusiveRange<T>(T min, T max, TResult failureResult)
    {
        return NotInExclusiveRange( min, max, Comparer<T>.Default, failureResult );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> NotInExclusiveRange<T>(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        return new IsNotInExclusiveRangeValidator<T, TResult>( min, max, comparer, failureResult );
    }
}
