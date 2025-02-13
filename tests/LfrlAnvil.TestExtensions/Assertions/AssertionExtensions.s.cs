using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LfrlAnvil.TestExtensions.Assertions;

public static class AssertionExtensions
{
    [Pure]
    public static TypeAssertionBuilder<T> TestType<T>(this T? subject, [CallerArgumentExpression( "subject" )] string context = "")
        where T : class
    {
        return new TypeAssertionBuilder<T>( context, subject );
    }

    [Pure]
    public static SubjectAssertion<Action> Test(
        this Action subject,
        Func<Exception?, Assertion> completionAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new CallAssertion( context, subject, completionAssertion );
    }

    [Pure]
    public static SubjectAssertion<Action> Test<T>(
        this Func<T> subject,
        Func<Exception?, Assertion> completionAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return Test( () => { subject(); }, completionAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<Action> Test(
        this Func<Task> subject,
        Func<Exception?, Assertion> completionAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return Test(
            () =>
            {
                try
                {
                    subject().Wait();
                }
                catch ( AggregateException exc )
                {
                    if ( exc.InnerExceptions.Count != 1 )
                        throw;

                    ExceptionDispatchInfo.Throw( exc.InnerExceptions[0] );
                }
            },
            completionAssertion,
            context );
    }

    [Pure]
    public static SubjectAssertion<Action> Test<T>(
        this Func<Task<T>> subject,
        Func<Exception?, Assertion> completionAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return Test( () => ( Task )subject(), completionAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<Action> Test(
        this Func<ValueTask> subject,
        Func<Exception?, Assertion> completionAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return Test( () => subject().AsTask(), completionAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<Action> Test<T>(
        this Func<ValueTask<T>> subject,
        Func<Exception?, Assertion> completionAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return Test( () => ( Task )subject().AsTask(), completionAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNull<T>(this T? subject, [CallerArgumentExpression( "subject" )] string context = "")
        where T : class
    {
        return new NullAssertion<T?>( context, subject, expected: true );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNull<T>(this T? subject, [CallerArgumentExpression( "subject" )] string context = "")
        where T : struct
    {
        return new NullAssertion<T?>( context, subject, expected: true );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNotNull<T>(this T? subject, [CallerArgumentExpression( "subject" )] string context = "")
        where T : class
    {
        return new NullAssertion<T?>( context, subject, expected: false );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNotNull<T>(this T? subject, [CallerArgumentExpression( "subject" )] string context = "")
        where T : struct
    {
        return new NullAssertion<T?>( context, subject, expected: false );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNotNull<T>(
        this T? subject,
        Func<T, Assertion> continuation,
        [CallerArgumentExpression( "subject" )] string context = "")
        where T : class
    {
        return subject.TestNotNull( context ).Then( x => continuation( x! ) );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNotNull<T>(
        this T? subject,
        Func<T, Assertion> continuation,
        [CallerArgumentExpression( "subject" )] string context = "")
        where T : struct
    {
        return subject.TestNotNull( context ).Then( x => continuation( x!.Value ) );
    }

    [Pure]
    public static SubjectAssertion<bool> TestTrue(this bool subject, [CallerArgumentExpression( "subject" )] string context = "")
    {
        return new BoolAssertion( context, subject, true );
    }

    [Pure]
    public static SubjectAssertion<bool> TestFalse(this bool subject, [CallerArgumentExpression( "subject" )] string context = "")
    {
        return new BoolAssertion( context, subject, false );
    }

    [Pure]
    public static SubjectAssertion<T?> TestRefEquals<T>(
        this T? subject,
        object? value,
        [CallerArgumentExpression( "subject" )]
        string context = "")
        where T : class
    {
        return new RefEqualsAssertion<T?>( context, subject, value, expected: true );
    }

    [Pure]
    public static SubjectAssertion<T?> TestNotRefEquals<T>(
        this T? subject,
        object? value,
        [CallerArgumentExpression( "subject" )]
        string context = "")
        where T : class
    {
        return new RefEqualsAssertion<T?>( context, subject, value, expected: false );
    }

    [Pure]
    public static SubjectAssertion<T> TestEquals<T>(this T subject, T value, [CallerArgumentExpression( "subject" )] string context = "")
    {
        return new EqualityAssertion<T>( context, subject, value, expected: true );
    }

    [Pure]
    public static SubjectAssertion<T> TestNotEquals<T>(this T subject, T value, [CallerArgumentExpression( "subject" )] string context = "")
    {
        return new EqualityAssertion<T>( context, subject, value, expected: false );
    }

    [Pure]
    public static SubjectAssertion<T> TestLessThan<T>(this T subject, T value, [CallerArgumentExpression( "subject" )] string context = "")
    {
        return new ComparisonAssertion<T>( context, subject, value, ComparisonType.LessThan );
    }

    [Pure]
    public static SubjectAssertion<T> TestLessThanOrEqualTo<T>(
        this T subject,
        T value,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new ComparisonAssertion<T>( context, subject, value, ComparisonType.LessThanOrEqualTo );
    }

    [Pure]
    public static SubjectAssertion<T> TestGreaterThan<T>(
        this T subject,
        T value,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new ComparisonAssertion<T>( context, subject, value, ComparisonType.GreaterThan );
    }

    [Pure]
    public static SubjectAssertion<T> TestGreaterThanOrEqualTo<T>(
        this T subject,
        T value,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new ComparisonAssertion<T>( context, subject, value, ComparisonType.GreaterThanOrEqualTo );
    }

    [Pure]
    public static SubjectAssertion<T> TestInRange<T>(
        this T subject,
        T min,
        T max,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new RangeComparisonAssertion<T>( context, subject, min, max, expected: true );
    }

    [Pure]
    public static SubjectAssertion<T> TestNotInRange<T>(
        this T subject,
        T min,
        T max,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new RangeComparisonAssertion<T>( context, subject, min, max, expected: false );
    }

    [Pure]
    public static SubjectAssertion<T> TestFuzzyEquals<T>(
        this T subject,
        T value,
        T epsilon,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new ApproximationAssertion<T>( context, subject, value, epsilon );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestMatch(
        this string subject,
        Regex regex,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.AsMemory().TestMatch( regex, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestMatch(
        this ReadOnlyMemory<char> subject,
        Regex regex,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new RegexAssertion( context, subject, regex, match: true );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestMatch(
        this ReadOnlySpan<char> subject,
        Regex regex,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToString().TestMatch( regex, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestNotMatch(
        this string subject,
        Regex regex,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.AsMemory().TestNotMatch( regex, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestNotMatch(
        this ReadOnlyMemory<char> subject,
        Regex regex,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new RegexAssertion( context, subject, regex, match: false );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestNotMatch(
        this ReadOnlySpan<char> subject,
        Regex regex,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToString().TestNotMatch( regex, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestStartsWith(
        this string subject,
        string value,
        StringComparison comparison = StringComparison.Ordinal,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.AsMemory().TestStartsWith( value.AsMemory(), comparison, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestStartsWith(
        this ReadOnlyMemory<char> subject,
        ReadOnlyMemory<char> value,
        StringComparison comparison = StringComparison.Ordinal,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new LikeAssertion( context, subject, value, comparison, LikeAssertion.ComparisonType.StartsWith );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestStartsWith(
        this ReadOnlySpan<char> subject,
        ReadOnlySpan<char> value,
        StringComparison comparison = StringComparison.Ordinal,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToString().TestStartsWith( value.ToString(), comparison, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestContains(
        this string subject,
        string value,
        StringComparison comparison = StringComparison.Ordinal,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.AsMemory().TestContains( value.AsMemory(), comparison, context );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestContains(
        this ReadOnlyMemory<char> subject,
        ReadOnlyMemory<char> value,
        StringComparison comparison = StringComparison.Ordinal,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new LikeAssertion( context, subject, value, comparison, LikeAssertion.ComparisonType.Contains );
    }

    [Pure]
    public static SubjectAssertion<ReadOnlyMemory<char>> TestContains(
        this ReadOnlySpan<char> subject,
        ReadOnlySpan<char> value,
        StringComparison comparison = StringComparison.Ordinal,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToString().TestContains( value.ToString(), comparison, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAll<T>(
        this IEnumerable<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new ForEachAssertion<T>( context, subject, elementAssertion, expectAll: true );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAll<T>(
        this ReadOnlyMemory<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAll( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAll<T>(
        this ReadOnlySpan<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAll( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAll<T>(
        this Memory<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAll( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAll<T>(
        this Span<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAll( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAny<T>(
        this IEnumerable<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new ForEachAssertion<T>( context, subject, elementAssertion, expectAll: false );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAny<T>(
        this ReadOnlyMemory<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAny( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAny<T>(
        this ReadOnlySpan<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAny( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAny<T>(
        this Memory<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAny( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestAny<T>(
        this Span<T> subject,
        Func<T, int, Assertion> elementAssertion,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestAny( elementAssertion, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this IEnumerable<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new SequenceAssertion<T>( context, subject, elementAssertions.ToArray(), SequenceComparisonType.Equal );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this Memory<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this Span<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this Memory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSequence<T>(
        this Span<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestEmpty<T>(
        this IEnumerable<T> subject,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( Array.Empty<T>(), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestEmpty<T>(
        this ReadOnlyMemory<T> subject,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( Array.Empty<T>(), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestEmpty<T>(
        this ReadOnlySpan<T> subject,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( Array.Empty<T>(), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestEmpty<T>(
        this Memory<T> subject,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( Array.Empty<T>(), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestEmpty<T>(
        this Span<T> subject,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestSequence( Array.Empty<T>(), context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this IEnumerable<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new SequenceAssertion<T>( context, subject, elementAssertions.ToArray(), SequenceComparisonType.Contains );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this Memory<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this Span<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this Memory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsSequence<T>(
        this Span<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this IEnumerable<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new SequenceAssertion<T>( context, subject, elementAssertions.ToArray(), SequenceComparisonType.ContainsContiguous );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsContiguousSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsContiguousSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this Memory<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsContiguousSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this Span<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestContainsContiguousSequence( elementAssertions, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsContiguousSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsContiguousSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsContiguousSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this Memory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsContiguousSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestContainsContiguousSequence<T>(
        this Span<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.TestContainsContiguousSequence(
            values.Select( x => ( Func<T, int, Assertion> )((element, _) => element.TestEquals( x )) ),
            context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSetEqual<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new SetAssertion<T>( context, subject, values.ToArray(), exact: true );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSetEqual<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSetEqual( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSetEqual<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSetEqual( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSetEqual<T>(
        this Memory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSetEqual( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSetEqual<T>(
        this Span<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSetEqual( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSupersetOf<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return new SetAssertion<T>( context, subject, values.ToArray(), exact: false );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSupersetOf<T>(
        this ReadOnlyMemory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSupersetOf( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSupersetOf<T>(
        this ReadOnlySpan<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSupersetOf( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSupersetOf<T>(
        this Memory<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSupersetOf( values, context );
    }

    [Pure]
    public static SubjectAssertion<IEnumerable<T>> TestSupersetOf<T>(
        this Span<T> subject,
        IEnumerable<T> values,
        [CallerArgumentExpression( "subject" )]
        string context = "")
    {
        return subject.ToArray().TestSupersetOf( values, context );
    }

    // TODO: remove 's' at the end
    [Pure]
    public static SubjectAssertion<T> TestReceivedCalls<T>(
        this T subject,
        Action<T> assertion,
        [CallerArgumentExpression( "subject" )]
        string context = "",
        [CallerArgumentExpression( "assertion" )]
        string subContext = "")
        where T : class
    {
        return new ReceivedCallsAssertion<T>( context, subContext, subject, assertion, count: null );
    }

    [Pure]
    public static SubjectAssertion<T> TestReceivedCalls<T>(
        this T subject,
        Action<T> assertion,
        int count,
        [CallerArgumentExpression( "subject" )]
        string context = "",
        [CallerArgumentExpression( "assertion" )]
        string subContext = "")
        where T : class
    {
        return new ReceivedCallsAssertion<T>( context, subContext, subject, assertion, count: count );
    }

    [Pure]
    public static SubjectAssertion<T> TestDidNotReceiveCall<T>(
        this T subject,
        Action<T> assertion,
        [CallerArgumentExpression( "subject" )]
        string context = "",
        [CallerArgumentExpression( "assertion" )]
        string subContext = "")
        where T : class
    {
        return subject.TestReceivedCalls( assertion, 0, context, subContext );
    }

    [Pure]
    public static int CallCount<TDelegate>(this TDelegate @delegate)
        where TDelegate : Delegate
    {
        return @delegate.ReceivedCalls().Count();
    }

    [Pure]
    public static DelegateCall CallAt<TDelegate>(this TDelegate @delegate, int index)
        where TDelegate : Delegate
    {
        return new DelegateCall( @delegate.ReceivedCalls().ElementAtOrDefault( index ) );
    }

    [Pure]
    internal static string Indent(this string value)
    {
        return value.Replace( Environment.NewLine, $"{Environment.NewLine}  " );
    }
}
