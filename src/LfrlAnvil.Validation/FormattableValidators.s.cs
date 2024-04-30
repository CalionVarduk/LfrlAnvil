using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation;

/// <summary>
/// Creates instances of <see cref="IValidator{T,TResult}"/> type with <see cref="ValidationMessage{TResource}"/> result.
/// </summary>
/// <typeparam name="TResource">Validator message's resource type.</typeparam>
public static class FormattableValidators<TResource>
{
    /// <summary>
    /// Creates a new <see cref="PassingValidator{T,TResult}"/> instance.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="PassingValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Pass<T>()
    {
        return Validators<ValidationMessage<TResource>>.Pass<T>();
    }

    /// <summary>
    /// Creates a new <see cref="FailingValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="FailingValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Fail<T>(TResource resource)
    {
        return Fail<T>( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="FailingValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="FailingValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Fail<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Fail<T>( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Expected minimum number of elements.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MinElementCountValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minCount"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MinElementCount<T>(int minCount, TResource resource)
    {
        return MinElementCount<T>( minCount, ValidationMessage.Create( resource, minCount ) );
    }

    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Expected minimum number of elements.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MinElementCountValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minCount"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MinElementCount<T>(
        int minCount,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MinElementCount<T>( minCount, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="MaxElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MaxElementCountValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxCount"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MaxElementCount<T>(int maxCount, TResource resource)
    {
        return MaxElementCount<T>( maxCount, ValidationMessage.Create( resource, maxCount ) );
    }

    /// <summary>
    /// Creates a new <see cref="MaxElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MaxElementCountValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxCount"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MaxElementCount<T>(
        int maxCount,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MaxElementCount<T>( maxCount, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="count">Expected exact number of elements.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountExactValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ExactElementCount<T>(int count, TResource resource)
    {
        return ExactElementCount<T>( count, ValidationMessage.Create( resource, count ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="count">Expected exact number of elements.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountExactValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ExactElementCount<T>(
        int count,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.ExactElementCount<T>( count, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minCount"/> is not in [<b>0</b>, <paramref name="maxCount"/>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ElementCountInRange<T>(
        int minCount,
        int maxCount,
        TResource resource)
    {
        return ElementCountInRange<T>( minCount, maxCount, ValidationMessage.Create( resource, minCount, maxCount ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minCount"/> is not in [<b>0</b>, <paramref name="maxCount"/>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ElementCountInRange<T>(
        int minCount,
        int maxCount,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.ElementCountInRange<T>( minCount, maxCount, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance
    /// with <see cref="IsElementCountExactValidator{T,TResult}.Count"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountExactValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> Empty<T>(TResource resource)
    {
        return Empty<T>( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance
    /// with <see cref="IsElementCountExactValidator{T,TResult}.Count"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IsElementCountExactValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> Empty<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Empty<T>( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance
    /// with <see cref="MinElementCountValidator{T,TResult}.MinCount"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MinElementCountValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> NotEmpty<T>(TResource resource)
    {
        return NotEmpty<T>( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance
    /// with <see cref="MinElementCountValidator{T,TResult}.MinCount"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MinElementCountValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> NotEmpty<T>(
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEmpty<T>( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Expected minimum <see cref="String.Length"/>.</param>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="MinLengthValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minLength"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MinLength(int minLength, TResource resource)
    {
        return MinLength( minLength, ValidationMessage.Create( resource, minLength ) );
    }

    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Expected minimum <see cref="String.Length"/>.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="MinLengthValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minLength"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MinLength(int minLength, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MinLength( minLength, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="MaxLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="maxLength">Expected maximum <see cref="String.Length"/>.</param>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="MaxLengthValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxLength"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MaxLength(int maxLength, TResource resource)
    {
        return MaxLength( maxLength, ValidationMessage.Create( resource, maxLength ) );
    }

    /// <summary>
    /// Creates a new <see cref="MaxLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="maxLength">Expected maximum <see cref="String.Length"/>.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="MaxLengthValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxLength"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MaxLength(int maxLength, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MaxLength( maxLength, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="length">Expected exact <see cref="String.Length"/>.</param>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsLengthExactValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> ExactLength(int length, TResource resource)
    {
        return ExactLength( length, ValidationMessage.Create( resource, length ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="length">Expected exact <see cref="String.Length"/>.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsLengthExactValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> ExactLength(int length, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.ExactLength( length, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthInRangeValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Minimum expected <see cref="String.Length"/>.</param>
    /// <param name="maxLength">Maximum expected <see cref="String.Length"/>.</param>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsLengthInRangeValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minLength"/> is not in [<b>0</b>, <paramref name="maxLength"/>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> LengthInRange(int minLength, int maxLength, TResource resource)
    {
        return LengthInRange( minLength, maxLength, ValidationMessage.Create( resource, minLength, maxLength ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthInRangeValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Minimum expected <see cref="String.Length"/>.</param>
    /// <param name="maxLength">Maximum expected <see cref="String.Length"/>.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsLengthInRangeValidator{TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minLength"/> is not in [<b>0</b>, <paramref name="maxLength"/>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> LengthInRange(
        int minLength,
        int maxLength,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LengthInRange( minLength, maxLength, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance
    /// with <see cref="IsLengthExactValidator{TResult}.Length"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsLengthExactValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Empty(TResource resource)
    {
        return Empty( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance
    /// with <see cref="IsLengthExactValidator{TResult}.Length"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsLengthExactValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Empty(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Empty( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance
    /// with <see cref="MinLengthValidator{TResult}.MinLength"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="MinLengthValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotEmpty(TResource resource)
    {
        return NotEmpty( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance
    /// with <see cref="MinLengthValidator{TResult}.MinLength"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="MinLengthValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotEmpty(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEmpty( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotWhiteSpace(TResource resource)
    {
        return NotWhiteSpace( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotWhiteSpace(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotWhiteSpace( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotMultilineValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsNotMultilineValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMultiline(TResource resource)
    {
        return NotMultiline( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotMultilineValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsNotMultilineValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMultiline(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotMultiline( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsRegexMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to match.</param>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsRegexMatchedValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Match(Regex regex, TResource resource)
    {
        return Match( regex, ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsRegexMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to match.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsRegexMatchedValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Match(Regex regex, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Match( regex, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsRegexNotMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to not match.</param>
    /// <param name="resource">Failure resource.</param>
    /// <returns>New <see cref="IsRegexNotMatchedValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMatch(Regex regex, TResource resource)
    {
        return NotMatch( regex, ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsRegexNotMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to not match.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <returns>New <see cref="IsRegexNotMatchedValidator{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMatch(Regex regex, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotMatch( regex, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNullValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Null<T>(TResource resource)
    {
        return Null<T>( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNullValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Null<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Null<T>( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotNullValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotNull<T>(TResource resource)
    {
        return NotNull<T>( ValidationMessage.Create( resource ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotNullValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotNull<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotNull<T>( failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsEqualToValidator{T,TResult}"/> instance with <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(T determinant, TResource resource)
    {
        return EqualTo( determinant, EqualityComparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsEqualToValidator{T,TResult}"/> instance with <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.EqualTo( determinant, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(T determinant, IEqualityComparer<T> comparer, TResource resource)
    {
        return EqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(
        T determinant,
        IEqualityComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.EqualTo( determinant, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(T determinant, TResource resource)
    {
        return NotEqualTo( determinant, EqualityComparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEqualTo( determinant, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(
        T determinant,
        IEqualityComparer<T> comparer,
        TResource resource)
    {
        return NotEqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(
        T determinant,
        IEqualityComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEqualTo( determinant, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(T determinant, TResource resource)
    {
        return LessThan( determinant, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThan( determinant, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(T determinant, IComparer<T> comparer, TResource resource)
    {
        return LessThan( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThan( determinant, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(T determinant, TResource resource)
    {
        return LessThanOrEqualTo( determinant, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(
        T determinant,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThanOrEqualTo( determinant, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(T determinant, IComparer<T> comparer, TResource resource)
    {
        return LessThanOrEqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsLessThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThanOrEqualTo( determinant, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(T determinant, TResource resource)
    {
        return GreaterThan( determinant, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThan( determinant, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(T determinant, IComparer<T> comparer, TResource resource)
    {
        return GreaterThan( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThan( determinant, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(T determinant, TResource resource)
    {
        return GreaterThanOrEqualTo( determinant, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(
        T determinant,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThanOrEqualTo( determinant, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(
        T determinant,
        IComparer<T> comparer,
        TResource resource)
    {
        return GreaterThanOrEqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsGreaterThanOrEqualToValidator{T,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThanOrEqualTo( determinant, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsInRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(T min, T max, TResource resource)
    {
        return InRange( min, max, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsInRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(T min, T max, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.InRange( min, max, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(T min, T max, IComparer<T> comparer, TResource resource)
    {
        return InRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(
        T min,
        T max,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.InRange( min, max, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(T min, T max, TResource resource)
    {
        return InExclusiveRange( min, max, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(T min, T max, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.InExclusiveRange( min, max, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(T min, T max, IComparer<T> comparer, TResource resource)
    {
        return InExclusiveRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(
        T min,
        T max,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.InExclusiveRange( min, max, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(T min, T max, TResource resource)
    {
        return NotInRange( min, max, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(T min, T max, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotInRange( min, max, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(T min, T max, IComparer<T> comparer, TResource resource)
    {
        return NotInRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value to compare with.</param>
    /// <param name="max">Maximum value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(
        T min,
        T max,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotInRange( min, max, comparer, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInExclusiveRange<T>(T min, T max, TResource resource)
    {
        return NotInExclusiveRange( min, max, Comparer<T>.Default, resource );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInExclusiveRange<T>(
        T min,
        T max,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotInExclusiveRange( min, max, failureMessage );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="resource">Failure resource.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInExclusiveRange<T>(
        T min,
        T max,
        IComparer<T> comparer,
        TResource resource)
    {
        return NotInExclusiveRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureMessage">Failure message.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInExclusiveRange<T>(
        T min,
        T max,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotInExclusiveRange( min, max, comparer, failureMessage );
    }
}
