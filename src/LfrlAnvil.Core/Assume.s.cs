using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

/// <summary>
/// Contains helper methods for <see cref="ConditionalAttribute"/> <b>DEBUG</b> assertions.
/// </summary>
/// <remarks>See <see cref="Debug.Assert(Boolean,String)"/> for more information.</remarks>
public static class Assume
{
    /// <summary>
    /// Assumes that <paramref name="param"/> is null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsNull<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( param is null, ExceptionResources.AssumedNull( param, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is not null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsNotNull<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( param is not null, ExceptionResources.AssumedNotNull( paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> <see cref="Enum"/> is defined.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <remarks>See <see cref="Enum.IsDefined{T}(T)"/> for more information.</remarks>
    [Conditional( "DEBUG" )]
    public static void IsDefined<T>(T param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct, Enum
    {
        Debug.Assert( Enum.IsDefined( param ), ExceptionResources.AssumedDefinedEnum( param, typeof( T ), paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void Equals<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        Debug.Assert( param.Equals( value ), ExceptionResources.AssumedEqualTo( param, value, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is not equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void NotEquals<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        Debug.Assert( ! param.Equals( value ), ExceptionResources.AssumedNotEqualTo( param, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is greater than <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsGreaterThan<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( Comparer<T>.Default.Compare( param, value ) > 0, ExceptionResources.AssumedGreaterThan( param, value, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is greater than or equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsGreaterThanOrEqualTo<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, value ) >= 0,
            ExceptionResources.AssumedGreaterThanOrEqualTo( param, value, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is less than <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsLessThan<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( Comparer<T>.Default.Compare( param, value ) < 0, ExceptionResources.AssumedLessThan( param, value, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is less than or equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsLessThanOrEqualTo<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, value ) <= 0,
            ExceptionResources.AssumedLessThanOrEqualTo( param, value, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsInRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) >= 0 && Comparer<T>.Default.Compare( param, max ) <= 0,
            ExceptionResources.AssumedInRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is not between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsNotInRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) < 0 || Comparer<T>.Default.Compare( param, max ) > 0,
            ExceptionResources.AssumedNotInRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsInExclusiveRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) > 0 && Comparer<T>.Default.Compare( param, max ) < 0,
            ExceptionResources.AssumedInExclusiveRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is not exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsNotInExclusiveRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) <= 0 || Comparer<T>.Default.Compare( param, max ) >= 0,
            ExceptionResources.AssumedNotInExclusiveRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsEmpty<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( ! param.Any(), ExceptionResources.AssumedEmpty( paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> is not empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void IsNotEmpty<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( param.Any(), ExceptionResources.AssumedNotEmpty( paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> contains at least <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Minimum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void ContainsAtLeast<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( param.ContainsAtLeast( count ), ExceptionResources.AssumedToContainAtLeast( count, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> contains at most <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Maximum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void ContainsAtMost<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( param.ContainsAtMost( count ), ExceptionResources.AssumedToContainAtMost( count, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> contains between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void ContainsInRange<T>(
        IEnumerable<T> param,
        int minCount,
        int maxCount,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert(
            param.ContainsInRange( minCount, maxCount ),
            ExceptionResources.AssumedToContainInRange( minCount, maxCount, paramName ) );
    }

    /// <summary>
    /// Assumes that <paramref name="param"/> contains exactly <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Exact expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    [Conditional( "DEBUG" )]
    public static void ContainsExactly<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        Debug.Assert( param.ContainsExactly( count ), ExceptionResources.AssumedToContainExactly( count, paramName ) );
    }

    /// <summary>
    /// Assumes that this call is unreachable.
    /// </summary>
    /// <param name="description">Optional description of the error.</param>
    [Conditional( "DEBUG" )]
    [DoesNotReturn]
    public static void Unreachable(string? description = null)
    {
        Debug.Fail( description ?? ExceptionResources.AssumedCodeToBeUnreachable );
    }

    /// <summary>
    /// Assumes that the provided <paramref name="condition"/> is <b>true</b>.
    /// </summary>
    /// <param name="condition">Condition to verify.</param>
    /// <param name="description">Optional description of the error.</param>
    [Conditional( "DEBUG" )]
    public static void True(bool condition, [CallerArgumentExpression( "condition" )] string description = "")
    {
        Debug.Assert( condition, ExceptionResources.AssumedToBeTrue( description ) );
    }

    /// <summary>
    /// Assumes that the provided <paramref name="condition"/> is <b>false</b>.
    /// </summary>
    /// <param name="condition">Condition to verify.</param>
    /// <param name="description">Optional description of the error.</param>
    [Conditional( "DEBUG" )]
    public static void False(bool condition, [CallerArgumentExpression( "condition" )] string description = "")
    {
        Debug.Assert( ! condition, ExceptionResources.AssumedToBeFalse( description ) );
    }

    /// <summary>
    /// Invokes the provided <paramref name="assumption"/> delegate
    /// when the specified <paramref name="condition"/> evaluates to <b>true</b>.
    /// </summary>
    /// <param name="condition">Condition to evaluate.</param>
    /// <param name="assumption">Delegate to invoke conditionally when <paramref name="condition"/> evaluates to <b>true</b>.</param>
    [Conditional( "DEBUG" )]
    public static void Conditional(bool condition, Action assumption)
    {
        if ( condition )
            assumption();
    }

    /// <summary>
    /// Invokes the provided <paramref name="assumptionIfTrue"/> delegate
    /// when the specified <paramref name="condition"/> evaluates to <b>true</b>,
    /// otherwise invokes the provided <paramref name="assumptionIfFalse"/> delegate.
    /// </summary>
    /// <param name="condition">Condition to evaluate.</param>
    /// <param name="assumptionIfTrue">Delegate to invoke conditionally when <paramref name="condition"/> evaluates to <b>true</b>.</param>
    /// <param name="assumptionIfFalse">
    /// Delegate to invoke conditionally when <paramref name="condition"/> evaluates to <b>false</b>.
    /// </param>
    [Conditional( "DEBUG" )]
    public static void Conditional(bool condition, Action assumptionIfTrue, Action assumptionIfFalse)
    {
        if ( condition )
            assumptionIfTrue();
        else
            assumptionIfFalse();
    }
}
