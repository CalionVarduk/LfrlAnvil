using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

public static class Assume
{
    [Conditional( "DEBUG" )]
    public static void IsNull<T>(T? param, string paramName)
    {
        Debug.Assert( param is null, ExceptionResources.AssumedNull( param, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsNotNull<T>([NotNull] T? param, string paramName)
    {
        Debug.Assert( param is not null, ExceptionResources.AssumedNotNull( paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsDefined<T>(T param, string paramName)
        where T : struct, Enum
    {
        Debug.Assert( Enum.IsDefined( param ), ExceptionResources.AssumedDefinedEnum( param, typeof( T ), paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void Equals<T>(T param, T? value, string paramName)
        where T : notnull
    {
        Debug.Assert( param.Equals( value ), ExceptionResources.AssumedEqualTo( param, value, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void NotEquals<T>(T param, T? value, string paramName)
        where T : notnull
    {
        Debug.Assert( ! param.Equals( value ), ExceptionResources.AssumedNotEqualTo( param, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsGreaterThan<T>(T param, T? value, string paramName)
    {
        Debug.Assert( Comparer<T>.Default.Compare( param, value ) > 0, ExceptionResources.AssumedGreaterThan( param, value, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsGreaterThanOrEqualTo<T>(T param, T? value, string paramName)
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, value ) >= 0,
            ExceptionResources.AssumedGreaterThanOrEqualTo( param, value, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsLessThan<T>(T param, T? value, string paramName)
    {
        Debug.Assert( Comparer<T>.Default.Compare( param, value ) < 0, ExceptionResources.AssumedLessThan( param, value, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsLessThanOrEqualTo<T>(T param, T? value, string paramName)
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, value ) <= 0,
            ExceptionResources.AssumedLessThanOrEqualTo( param, value, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsInRange<T>(T param, T min, T max, string paramName)
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) >= 0 && Comparer<T>.Default.Compare( param, max ) <= 0,
            ExceptionResources.AssumedInRange( param, min, max, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsNotInRange<T>(T param, T min, T max, string paramName)
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) < 0 || Comparer<T>.Default.Compare( param, max ) > 0,
            ExceptionResources.AssumedNotInRange( param, min, max, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsInExclusiveRange<T>(T param, T min, T max, string paramName)
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) > 0 && Comparer<T>.Default.Compare( param, max ) < 0,
            ExceptionResources.AssumedInExclusiveRange( param, min, max, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsNotInExclusiveRange<T>(T param, T min, T max, string paramName)
    {
        Debug.Assert(
            Comparer<T>.Default.Compare( param, min ) <= 0 || Comparer<T>.Default.Compare( param, max ) >= 0,
            ExceptionResources.AssumedNotInExclusiveRange( param, min, max, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsEmpty<T>(IEnumerable<T> param, string paramName)
    {
        Debug.Assert( ! param.Any(), ExceptionResources.AssumedEmpty( paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsNotEmpty<T>(IEnumerable<T> param, string paramName)
    {
        Debug.Assert( param.Any(), ExceptionResources.AssumedNotEmpty( paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void ContainsAtLeast<T>(IEnumerable<T> param, int count, string paramName)
    {
        Debug.Assert( param.ContainsAtLeast( count ), ExceptionResources.AssumedToContainAtLeast( count, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void ContainsAtMost<T>(IEnumerable<T> param, int count, string paramName)
    {
        Debug.Assert( param.ContainsAtMost( count ), ExceptionResources.AssumedToContainAtMost( count, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void ContainsInRange<T>(IEnumerable<T> param, int minCount, int maxCount, string paramName)
    {
        Debug.Assert(
            param.ContainsInRange( minCount, maxCount ),
            ExceptionResources.AssumedToContainInRange( minCount, maxCount, paramName ) );
    }

    [Conditional( "DEBUG" )]
    public static void ContainsExactly<T>(IEnumerable<T> param, int count, string paramName)
    {
        Debug.Assert( param.ContainsExactly( count ), ExceptionResources.AssumedToContainExactly( count, paramName ) );
    }

    [Conditional( "DEBUG" )]
    [DoesNotReturn]
    public static void Unreachable(string? description = null)
    {
        Debug.Fail( description ?? ExceptionResources.AssumedCodeToBeUnreachable );
    }

    [Conditional( "DEBUG" )]
    public static void True(bool condition, string description)
    {
        Debug.Assert( condition, description );
    }

    [Conditional( "DEBUG" )]
    public static void False(bool condition, string description)
    {
        Debug.Assert( ! condition, description );
    }

    [Conditional( "DEBUG" )]
    public static void Conditional(bool condition, Action assumption)
    {
        if ( condition )
            assumption();
    }

    [Conditional( "DEBUG" )]
    public static void Conditional(bool condition, Action assumptionIfTrue, Action assumptionIfFalse)
    {
        if ( condition )
            assumptionIfTrue();
        else
            assumptionIfFalse();
    }
}
