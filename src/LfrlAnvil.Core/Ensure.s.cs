using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class Ensure
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNull<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param is not null )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNull<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( param.HasValue )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNull<T>(T? param, IEqualityComparer<T> comparer, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );

        if ( ! comparer.Equals( param, default ) )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNull<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param is null )
            ExceptionThrower.Throw( Exceptions.Null( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNull<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( ! param.HasValue )
            ExceptionThrower.Throw( Exceptions.Null( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNull<T>(T? param, IEqualityComparer<T> comparer, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            return;

        if ( comparer.Equals( param, default ) )
            ExceptionThrower.Throw( Exceptions.Null( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsDefault<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( Generic<T>.IsNotDefault( param ) )
            ExceptionThrower.Throw( Exceptions.NotDefault( param, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotDefault<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( Generic<T>.IsDefault( param ) )
            ExceptionThrower.Throw( Exceptions.Default( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsOfType( param, typeof( T ), paramName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( type != param.GetType() )
            ExceptionThrower.Throw( Exceptions.NotOfType( type, param.GetType(), paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsNotOfType( param, typeof( T ), paramName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( type == param.GetType() )
            ExceptionThrower.Throw( Exceptions.OfType( type, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInstanceOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsInstanceOfType( param, typeof( T ), paramName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInstanceOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( ! type.IsInstanceOfType( param ) )
            ExceptionThrower.Throw( Exceptions.NotInstanceOfType( type, param.GetType(), paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInstanceOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsNotInstanceOfType( param, typeof( T ), paramName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInstanceOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( type.IsInstanceOfType( param ) )
            ExceptionThrower.Throw( Exceptions.InstanceOfType( type, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsDefined<T>(T param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct, Enum
    {
        if ( ! Enum.IsDefined( param ) )
            ExceptionThrower.Throw( Exceptions.EnumNotDefined( param, typeof( T ), paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Equals<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( ! param.Equals( value ) )
            ExceptionThrower.Throw( Exceptions.NotEqualTo( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Equals<T>(
        T param,
        T? value,
        IEqualityComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! comparer.Equals( param, value ) )
            ExceptionThrower.Throw( Exceptions.NotEqualTo( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotEquals<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( param.Equals( value ) )
            ExceptionThrower.Throw( Exceptions.EqualTo( value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotEquals<T>(
        T param,
        T? value,
        IEqualityComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Equals( param, value ) )
            ExceptionThrower.Throw( Exceptions.EqualTo( value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void RefEquals<T>(
        [NotNullIfNotNull( "value" )] T? param,
        T? value,
        [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( ! ReferenceEquals( param, value ) )
            ExceptionThrower.Throw( Exceptions.NotRefEqualTo( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotRefEquals<T>(T? param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( ReferenceEquals( param, value ) )
            ExceptionThrower.Throw( Exceptions.RefEqualTo( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsGreaterThan<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) <= 0 )
            ExceptionThrower.Throw( Exceptions.NotGreaterThan( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsGreaterThan<T>(
        T param,
        T? value,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, value ) <= 0 )
            ExceptionThrower.Throw( Exceptions.NotGreaterThan( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsGreaterThanOrEqualTo<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) < 0 )
            ExceptionThrower.Throw( Exceptions.NotGreaterThanOrEqual( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsGreaterThanOrEqualTo<T>(
        T param,
        T? value,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, value ) < 0 )
            ExceptionThrower.Throw( Exceptions.NotGreaterThanOrEqual( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThan<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThan( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThan<T>(T param, T? value, IComparer<T> comparer, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, value ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThan( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThanOrEqualTo<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) > 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThanOrEqual( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThanOrEqualTo<T>(
        T param,
        T? value,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, value ) > 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThanOrEqual( param, value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) < 0 || param.CompareTo( max ) > 0 )
            ExceptionThrower.Throw( Exceptions.NotInRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInRange<T>(
        T param,
        T min,
        T max,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, min ) < 0 || comparer.Compare( param, max ) > 0 )
            ExceptionThrower.Throw( Exceptions.NotInRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) >= 0 && param.CompareTo( max ) <= 0 )
            ExceptionThrower.Throw( Exceptions.InRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInRange<T>(
        T param,
        T min,
        T max,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, min ) >= 0 && comparer.Compare( param, max ) <= 0 )
            ExceptionThrower.Throw( Exceptions.InRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInExclusiveRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) <= 0 || param.CompareTo( max ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotInExclusiveRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInExclusiveRange<T>(
        T param,
        T min,
        T max,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, min ) <= 0 || comparer.Compare( param, max ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotInExclusiveRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInExclusiveRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) > 0 && param.CompareTo( max ) < 0 )
            ExceptionThrower.Throw( Exceptions.InExclusiveRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInExclusiveRange<T>(
        T param,
        T min,
        T max,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, min ) > 0 && comparer.Compare( param, max ) < 0 )
            ExceptionThrower.Throw( Exceptions.InExclusiveRange( param, min, max, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsEmpty<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Any() )
            ExceptionThrower.Throw( Exceptions.NotEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsEmpty<T>(IReadOnlyCollection<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsEmpty() )
            ExceptionThrower.Throw( Exceptions.NotEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsEmpty(string param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Length > 0 )
            ExceptionThrower.Throw( Exceptions.NotEmpty( param, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotEmpty<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.Any() )
            ExceptionThrower.Throw( Exceptions.Empty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotEmpty<T>(IReadOnlyCollection<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.IsEmpty() )
            ExceptionThrower.Throw( Exceptions.Empty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotEmpty(string param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Length == 0 )
            ExceptionThrower.Throw( Exceptions.Empty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNullOrEmpty<T>(IEnumerable<T>? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NotNullOrEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNullOrEmpty<T>(IReadOnlyCollection<T>? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NotNullOrEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNullOrEmpty(string? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! string.IsNullOrEmpty( param ) )
            ExceptionThrower.Throw( Exceptions.NotNullOrEmpty( param, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrEmpty<T>([NotNull] IEnumerable<T>? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NullOrEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrEmpty<T>(
        [NotNull] IReadOnlyCollection<T>? param,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NullOrEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrEmpty([NotNull] string? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( string.IsNullOrEmpty( param ) )
            ExceptionThrower.Throw( Exceptions.NullOrEmpty( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrWhiteSpace([NotNull] string? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( string.IsNullOrWhiteSpace( param ) )
            ExceptionThrower.Throw( Exceptions.NullOrWhiteSpace( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param.All( static e => e is not null ) )
            ExceptionThrower.Throw( Exceptions.NotContainsNull( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( param.All( static e => e.HasValue ) )
            ExceptionThrower.Throw( Exceptions.NotContainsNull( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsNull<T>(
        IEnumerable<T?> param,
        IEqualityComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            ExceptionThrower.Throw( Exceptions.NotContainsNull( paramName ) );

        if ( param.All( e => ! comparer.Equals( e, default ) ) )
            ExceptionThrower.Throw( Exceptions.NotContainsNull( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param.Any( static e => e is null ) )
            ExceptionThrower.Throw( Exceptions.ContainsNull( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( param.Any( static e => ! e.HasValue ) )
            ExceptionThrower.Throw( Exceptions.ContainsNull( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContainsNull<T>(
        IEnumerable<T?> param,
        IEqualityComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            return;

        if ( param.Any( e => comparer.Equals( e, default ) ) )
            ExceptionThrower.Throw( Exceptions.ContainsNull( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtLeast<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtLeast( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtLeast( count, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtLeast<T>(
        IReadOnlyCollection<T> param,
        int count,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtLeast( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtLeast( count, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtMost<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtMost( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtMost( count, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtMost<T>(
        IReadOnlyCollection<T> param,
        int count,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtMost( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtMost( count, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsInRange<T>(
        IEnumerable<T> param,
        int minCount,
        int maxCount,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsInRange( minCount, maxCount ) )
            ExceptionThrower.Throw( Exceptions.NotContainsInRange( minCount, maxCount, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsInRange<T>(
        IReadOnlyCollection<T> param,
        int minCount,
        int maxCount,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsInRange( minCount, maxCount ) )
            ExceptionThrower.Throw( Exceptions.NotContainsInRange( minCount, maxCount, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsExactly<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsExactly( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsExactly( count, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsExactly<T>(
        IReadOnlyCollection<T> param,
        int count,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsExactly( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsExactly( count, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Contains<T>(IEnumerable<T> param, T value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( ! param.Any( e => e.Equals( value ) ) )
            ExceptionThrower.Throw( Exceptions.NotContains( value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Contains<T>(
        IEnumerable<T> param,
        T value,
        IEqualityComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.Any( e => comparer.Equals( e, value ) ) )
            ExceptionThrower.Throw( Exceptions.NotContains( value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContains<T>(IEnumerable<T> param, T value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( param.Any( e => e.Equals( value ) ) )
            ExceptionThrower.Throw( Exceptions.Contains( value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContains<T>(
        IEnumerable<T> param,
        T value,
        IEqualityComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Any( e => comparer.Equals( e, value ) ) )
            ExceptionThrower.Throw( Exceptions.Contains( value, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ForAny<T>(
        IEnumerable<T> param,
        Func<T, bool> predicate,
        string? description = null,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.Any( predicate ) )
            ExceptionThrower.Throw( Exceptions.NotAny( description, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ForAny<T>(
        IEnumerable<T> param,
        Func<T, bool> predicate,
        Func<string>? descriptionProvider,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.Any( predicate ) )
            ExceptionThrower.Throw( Exceptions.NotAny( descriptionProvider?.Invoke(), paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ForAll<T>(
        IEnumerable<T> param,
        Func<T, bool> predicate,
        string? description = null,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.All( predicate ) )
            ExceptionThrower.Throw( Exceptions.NotAll( description, paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ForAll<T>(
        IEnumerable<T> param,
        Func<T, bool> predicate,
        Func<string>? descriptionProvider,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.All( predicate ) )
            ExceptionThrower.Throw( Exceptions.NotAll( descriptionProvider?.Invoke(), paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOrdered<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( ! param.IsOrdered() )
            ExceptionThrower.Throw( Exceptions.NotOrdered( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOrdered<T>(
        IEnumerable<T> param,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsOrdered( comparer ) )
            ExceptionThrower.Throw( Exceptions.NotOrdered( paramName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void True(bool condition, [CallerArgumentExpression( "condition" )] string description = "")
    {
        if ( ! condition )
            ExceptionThrower.Throw( Exceptions.False( description ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void True(bool condition, Func<string>? descriptionProvider)
    {
        if ( ! condition )
            ExceptionThrower.Throw( Exceptions.False( descriptionProvider?.Invoke() ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void False(bool condition, [CallerArgumentExpression( "condition" )] string description = "")
    {
        if ( condition )
            ExceptionThrower.Throw( Exceptions.True( description ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void False(bool condition, Func<string>? descriptionProvider)
    {
        if ( condition )
            ExceptionThrower.Throw( Exceptions.True( descriptionProvider?.Invoke() ) );
    }

    private static class Exceptions
    {
        public static ArgumentException NotNull<T>(T param, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotNull( param, paramName ), paramName );
        }

        public static ArgumentNullException Null(string paramName)
        {
            return new ArgumentNullException( paramName );
        }

        public static ArgumentException NotDefault<T>(T param, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedDefault( param, paramName ), paramName );
        }

        public static ArgumentException Default(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotDefault( paramName ), paramName );
        }

        public static ArgumentException NotOfType(Type type, Type actualType, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedOfType( type, actualType, paramName ), paramName );
        }

        public static ArgumentException OfType(Type type, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotOfType( type, paramName ), paramName );
        }

        public static ArgumentException NotInstanceOfType(Type type, Type actualType, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedInstanceOfType( type, actualType, paramName ), paramName );
        }

        public static ArgumentException InstanceOfType(Type type, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotInstanceOfType( type, paramName ), paramName );
        }

        public static ArgumentException EnumNotDefined<T>(T param, Type enumType, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedDefinedEnum( param, enumType, paramName ), paramName );
        }

        public static ArgumentException NotEqualTo<T>(T param, T expectedValue, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedEqualTo( param, expectedValue, paramName ), paramName );
        }

        public static ArgumentException EqualTo<T>(T expectedValue, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotEqualTo( expectedValue, paramName ), paramName );
        }

        public static ArgumentException NotRefEqualTo(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedRefEqualTo( paramName ), paramName );
        }

        public static ArgumentException RefEqualTo(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotRefEqualTo( paramName ), paramName );
        }

        public static ArgumentOutOfRangeException NotGreaterThan<T>(T param, T expectedValue, string paramName)
        {
            return new ArgumentOutOfRangeException(
                paramName,
                ExceptionResources.ExpectedGreaterThan( param, expectedValue, paramName ) );
        }

        public static ArgumentOutOfRangeException NotGreaterThanOrEqual<T>(T param, T expectedValue, string paramName)
        {
            return new ArgumentOutOfRangeException(
                paramName,
                ExceptionResources.ExpectedGreaterThanOrEqualTo( param, expectedValue, paramName ) );
        }

        public static ArgumentOutOfRangeException NotLessThan<T>(T param, T expectedValue, string paramName)
        {
            return new ArgumentOutOfRangeException( paramName, ExceptionResources.ExpectedLessThan( param, expectedValue, paramName ) );
        }

        public static ArgumentOutOfRangeException NotLessThanOrEqual<T>(T param, T expectedValue, string paramName)
        {
            return new ArgumentOutOfRangeException(
                paramName,
                ExceptionResources.ExpectedLessThanOrEqualTo( param, expectedValue, paramName ) );
        }

        public static ArgumentOutOfRangeException InRange<T>(T param, T min, T max, string paramName)
        {
            return new ArgumentOutOfRangeException( paramName, ExceptionResources.ExpectedNotInRange( param, min, max, paramName ) );
        }

        public static ArgumentOutOfRangeException NotInRange<T>(T param, T min, T max, string paramName)
        {
            return new ArgumentOutOfRangeException( paramName, ExceptionResources.ExpectedInRange( param, min, max, paramName ) );
        }

        public static ArgumentOutOfRangeException InExclusiveRange<T>(T param, T min, T max, string paramName)
        {
            return new ArgumentOutOfRangeException(
                paramName,
                ExceptionResources.ExpectedNotInExclusiveRange( param, min, max, paramName ) );
        }

        public static ArgumentOutOfRangeException NotInExclusiveRange<T>(T param, T min, T max, string paramName)
        {
            return new ArgumentOutOfRangeException(
                paramName,
                ExceptionResources.ExpectedInExclusiveRange( param, min, max, paramName ) );
        }

        public static ArgumentException NotEmpty(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedEmpty( paramName ), paramName );
        }

        public static ArgumentException NotEmpty(string param, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedEmpty( param, paramName ), paramName );
        }

        public static ArgumentException Empty(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotEmpty( paramName ), paramName );
        }

        public static ArgumentException NotNullOrEmpty(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNullOrEmpty( paramName ), paramName );
        }

        public static ArgumentException NotNullOrEmpty(string param, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNullOrEmpty( param, paramName ), paramName );
        }

        public static ArgumentException NullOrEmpty(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotNullOrEmpty( paramName ), paramName );
        }

        public static ArgumentException NullOrWhiteSpace(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedNotNullOrWhiteSpace( paramName ), paramName );
        }

        public static ArgumentException NotContainsNull(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToContainNull( paramName ), paramName );
        }

        public static ArgumentException ContainsNull(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToNotContainNull( paramName ), paramName );
        }

        public static ArgumentException NotContainsAtLeast(int count, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToContainAtLeast( count, paramName ), paramName );
        }

        public static ArgumentException NotContainsAtMost(int count, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToContainAtMost( count, paramName ), paramName );
        }

        public static ArgumentException NotContainsInRange(int minCount, int maxCount, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToContainInRange( minCount, maxCount, paramName ), paramName );
        }

        public static ArgumentException NotContainsExactly(int count, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToContainExactly( count, paramName ), paramName );
        }

        public static ArgumentException NotContains<T>(T value, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToContain( value, paramName ), paramName );
        }

        public static ArgumentException Contains<T>(T value, string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedToNotContain( value, paramName ), paramName );
        }

        public static ArgumentException NotAny(string? description, string paramName)
        {
            return new ArgumentException( description ?? ExceptionResources.ExpectedAnyToPassThePredicate( paramName ), paramName );
        }

        public static ArgumentException NotAll(string? description, string paramName)
        {
            return new ArgumentException( description ?? ExceptionResources.ExpectedAllToPassThePredicate( paramName ), paramName );
        }

        public static ArgumentException NotOrdered(string paramName)
        {
            return new ArgumentException( ExceptionResources.ExpectedOrdered( paramName ), paramName );
        }

        public static ArgumentException False(string? description)
        {
            return new ArgumentException( ExceptionResources.ExpectedToBeTrue( description ?? "condition" ) );
        }

        public static ArgumentException True(string? description)
        {
            return new ArgumentException( ExceptionResources.ExpectedToBeFalse( description ?? "condition" ) );
        }
    }
}
