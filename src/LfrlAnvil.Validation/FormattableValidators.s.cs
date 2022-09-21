using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LfrlAnvil.Validation;

public static class FormattableValidators<TResource>
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Pass<T>()
    {
        return Validators<ValidationMessage<TResource>>.Pass<T>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Fail<T>(TResource resource)
    {
        return Fail<T>( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Fail<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Fail<T>( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MinElementCount<T>(int minCount, TResource resource)
    {
        return MinElementCount<T>( minCount, ValidationMessage.Create( resource, minCount ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MinElementCount<T>(
        int minCount,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MinElementCount<T>( minCount, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MaxElementCount<T>(int maxCount, TResource resource)
    {
        return MaxElementCount<T>( maxCount, ValidationMessage.Create( resource, maxCount ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> MaxElementCount<T>(
        int maxCount,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MaxElementCount<T>( maxCount, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ExactElementCount<T>(int count, TResource resource)
    {
        return ExactElementCount<T>( count, ValidationMessage.Create( resource, count ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ExactElementCount<T>(
        int count,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.ExactElementCount<T>( count, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ElementCountInRange<T>(
        int minCount,
        int maxCount,
        TResource resource)
    {
        return ElementCountInRange<T>( minCount, maxCount, ValidationMessage.Create( resource, minCount, maxCount ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> ElementCountInRange<T>(
        int minCount,
        int maxCount,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.ElementCountInRange<T>( minCount, maxCount, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> Empty<T>(TResource resource)
    {
        return Empty<T>( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> Empty<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Empty<T>( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> NotEmpty<T>(TResource resource)
    {
        return NotEmpty<T>( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ValidationMessage<TResource>> NotEmpty<T>(
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEmpty<T>( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MinLength(int minLength, TResource resource)
    {
        return MinLength( minLength, ValidationMessage.Create( resource, minLength ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MinLength(int minLength, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MinLength( minLength, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MaxLength(int maxLength, TResource resource)
    {
        return MaxLength( maxLength, ValidationMessage.Create( resource, maxLength ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> MaxLength(int maxLength, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.MaxLength( maxLength, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> ExactLength(int length, TResource resource)
    {
        return ExactLength( length, ValidationMessage.Create( resource, length ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> ExactLength(int length, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.ExactLength( length, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> LengthInRange(int minLength, int maxLength, TResource resource)
    {
        return LengthInRange( minLength, maxLength, ValidationMessage.Create( resource, minLength, maxLength ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> LengthInRange(
        int minLength,
        int maxLength,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LengthInRange( minLength, maxLength, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Empty(TResource resource)
    {
        return Empty( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Empty(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Empty( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotEmpty(TResource resource)
    {
        return NotEmpty( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotEmpty(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEmpty( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotWhiteSpace(TResource resource)
    {
        return NotWhiteSpace( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotWhiteSpace(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotWhiteSpace( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMultiline(TResource resource)
    {
        return NotMultiline( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMultiline(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotMultiline( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Match(Regex regex, TResource resource)
    {
        return Match( regex, ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> Match(Regex regex, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Match( regex, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMatch(Regex regex, TResource resource)
    {
        return NotMatch( regex, ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<string, ValidationMessage<TResource>> NotMatch(Regex regex, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotMatch( regex, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Null<T>(TResource resource)
    {
        return Null<T>( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> Null<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.Null<T>( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotNull<T>(TResource resource)
    {
        return NotNull<T>( ValidationMessage.Create( resource ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotNull<T>(ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotNull<T>( failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(T determinant, TResource resource)
    {
        return EqualTo( determinant, EqualityComparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.EqualTo( determinant, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(T determinant, IEqualityComparer<T> comparer, TResource resource)
    {
        return EqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> EqualTo<T>(
        T determinant,
        IEqualityComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.EqualTo( determinant, comparer, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(T determinant, TResource resource)
    {
        return NotEqualTo( determinant, EqualityComparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEqualTo( determinant, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(
        T determinant,
        IEqualityComparer<T> comparer,
        TResource resource)
    {
        return NotEqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotEqualTo<T>(
        T determinant,
        IEqualityComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotEqualTo( determinant, comparer, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(T determinant, TResource resource)
    {
        return LessThan( determinant, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThan( determinant, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(T determinant, IComparer<T> comparer, TResource resource)
    {
        return LessThan( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThan<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThan( determinant, comparer, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(T determinant, TResource resource)
    {
        return LessThanOrEqualTo( determinant, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(
        T determinant,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThanOrEqualTo( determinant, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(T determinant, IComparer<T> comparer, TResource resource)
    {
        return LessThanOrEqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> LessThanOrEqualTo<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.LessThanOrEqualTo( determinant, comparer, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(T determinant, TResource resource)
    {
        return GreaterThan( determinant, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(T determinant, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThan( determinant, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(T determinant, IComparer<T> comparer, TResource resource)
    {
        return GreaterThan( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThan<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThan( determinant, comparer, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(T determinant, TResource resource)
    {
        return GreaterThanOrEqualTo( determinant, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(
        T determinant,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThanOrEqualTo( determinant, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(
        T determinant,
        IComparer<T> comparer,
        TResource resource)
    {
        return GreaterThanOrEqualTo( determinant, comparer, ValidationMessage.Create( resource, determinant ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> GreaterThanOrEqualTo<T>(
        T determinant,
        IComparer<T> comparer,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.GreaterThanOrEqualTo( determinant, comparer, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(T min, T max, TResource resource)
    {
        return InRange( min, max, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(T min, T max, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.InRange( min, max, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InRange<T>(T min, T max, IComparer<T> comparer, TResource resource)
    {
        return InRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(T min, T max, TResource resource)
    {
        return InExclusiveRange( min, max, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(T min, T max, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.InExclusiveRange( min, max, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> InExclusiveRange<T>(T min, T max, IComparer<T> comparer, TResource resource)
    {
        return InExclusiveRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(T min, T max, TResource resource)
    {
        return NotInRange( min, max, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(T min, T max, ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotInRange( min, max, failureMessage );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInRange<T>(T min, T max, IComparer<T> comparer, TResource resource)
    {
        return NotInRange( min, max, comparer, ValidationMessage.Create( resource, min, max ) );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInExclusiveRange<T>(T min, T max, TResource resource)
    {
        return NotInExclusiveRange( min, max, Comparer<T>.Default, resource );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, ValidationMessage<TResource>> NotInExclusiveRange<T>(
        T min,
        T max,
        ValidationMessage<TResource> failureMessage)
    {
        return Validators<ValidationMessage<TResource>>.NotInExclusiveRange( min, max, failureMessage );
    }

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
