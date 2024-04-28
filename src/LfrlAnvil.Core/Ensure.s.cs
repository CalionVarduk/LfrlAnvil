using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Contains helper methods for simple assertions.
/// </summary>
public static class Ensure
{
    /// <summary>
    /// Ensures that <paramref name="param"/> is null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not null.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNull<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param is not null )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not null.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNull<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( param.HasValue )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not null.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNull<T>(T? param, IEqualityComparer<T> comparer, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );

        if ( ! comparer.Equals( param, default ) )
            ExceptionThrower.Throw( Exceptions.NotNull( param, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentNullException">When <paramref name="param"/> is null.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNull<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param is null )
            ExceptionThrower.Throw( Exceptions.Null( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentNullException">When <paramref name="param"/> is null.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNull<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( ! param.HasValue )
            ExceptionThrower.Throw( Exceptions.Null( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentNullException">When <paramref name="param"/> is null.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNull<T>(T? param, IEqualityComparer<T> comparer, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            return;

        if ( comparer.Equals( param, default ) )
            ExceptionThrower.Throw( Exceptions.Null( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is equivalent to default.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not equivalent to default.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsDefault<T>(T? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( Generic<T>.IsNotDefault( param ) )
            ExceptionThrower.Throw( Exceptions.NotDefault( param, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not equivalent to default.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is equivalent to default.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotDefault<T>([NotNull] T? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( Generic<T>.IsDefault( param ) )
            ExceptionThrower.Throw( Exceptions.Default( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is exactly of the provided type.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Expected type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not exactly of the provided type.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsOfType( param, typeof( T ), paramName );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is exactly of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="type">Expected type.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> is not exactly of the provided <paramref name="type"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( type != param.GetType() )
            ExceptionThrower.Throw( Exceptions.NotOfType( type, param.GetType(), paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not exactly of the provided type.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Unexpected type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is exactly of the provided type.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsNotOfType( param, typeof( T ), paramName );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not exactly of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="type">Unexpected type.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> is exactly of the provided <paramref name="type"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( type == param.GetType() )
            ExceptionThrower.Throw( Exceptions.OfType( type, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is an instance of the provided type.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Expected type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not an instance of the provided type.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInstanceOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsInstanceOfType( param, typeof( T ), paramName );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is an instance of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="type">Expected type.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> is not an instance of the provided <paramref name="type"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInstanceOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( ! type.IsInstanceOfType( param ) )
            ExceptionThrower.Throw( Exceptions.NotInstanceOfType( type, param.GetType(), paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not an instance of the provided type.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Unexpected type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is an instance of the provided type.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInstanceOfType<T>(object param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        IsNotInstanceOfType( param, typeof( T ), paramName );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not an instance of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="type">Unexpected type.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> is an instance of the provided <paramref name="type"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInstanceOfType<T>(T param, Type type, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : notnull
    {
        if ( type.IsInstanceOfType( param ) )
            ExceptionThrower.Throw( Exceptions.InstanceOfType( type, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> <see cref="Enum"/> is defined.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not defined.</exception>
    /// <remarks>See <see cref="Enum.IsDefined{T}(T)"/> for more information.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsDefined<T>(T param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct, Enum
    {
        if ( ! Enum.IsDefined( param ) )
            ExceptionThrower.Throw( Exceptions.EnumNotDefined( param, typeof( T ), paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not equal to <paramref name="value"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Equals<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( ! param.Equals( value ) )
            ExceptionThrower.Throw( Exceptions.NotEqualTo( param, value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not equal to <paramref name="value"/>.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is not equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is equal to <paramref name="value"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotEquals<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( param.Equals( value ) )
            ExceptionThrower.Throw( Exceptions.EqualTo( value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is equal to <paramref name="value"/>.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is ref-equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not ref-equal to <paramref name="value"/>.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is not ref-equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is ref-equal to <paramref name="value"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotRefEquals<T>(T? param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( ReferenceEquals( param, value ) )
            ExceptionThrower.Throw( Exceptions.RefEqualTo( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is greater than <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not greater than <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsGreaterThan<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) <= 0 )
            ExceptionThrower.Throw( Exceptions.NotGreaterThan( param, value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is greater than <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer used for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not greater than <paramref name="value"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is greater than or equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not greater than or equal to <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsGreaterThanOrEqualTo<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) < 0 )
            ExceptionThrower.Throw( Exceptions.NotGreaterThanOrEqual( param, value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is greater than or equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer used for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not greater than or equal to <paramref name="value"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is less than <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not less than <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThan<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThan( param, value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is less than <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer used for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not less than <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThan<T>(T param, T? value, IComparer<T> comparer, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( comparer.Compare( param, value ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThan( param, value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is less than or equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not less than or equal to <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsLessThanOrEqualTo<T>(T param, T? value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( value ) > 0 )
            ExceptionThrower.Throw( Exceptions.NotLessThanOrEqual( param, value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is less than or equal to <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="value">Value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer used for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not less than or equal to <paramref name="value"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is between <b>0</b> and (<paramref name="count"/> - 1).
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="count">Number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not between <b>0</b> and (<paramref name="count"/> - 1).
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInIndexRange(int param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param < 0 || param >= count )
            ExceptionThrower.Throw( Exceptions.NotInRange( param, 0, count - 1, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) < 0 || param.CompareTo( max ) > 0 )
            ExceptionThrower.Throw( Exceptions.NotInRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is not between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) >= 0 && param.CompareTo( max ) <= 0 )
            ExceptionThrower.Throw( Exceptions.InRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsInExclusiveRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) <= 0 || param.CompareTo( max ) >= 0 )
            ExceptionThrower.Throw( Exceptions.NotInExclusiveRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is not exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is not exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotInExclusiveRange<T>(T param, T min, T max, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( param.CompareTo( min ) > 0 && param.CompareTo( max ) < 0 )
            ExceptionThrower.Throw( Exceptions.InExclusiveRange( param, min, max, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="param">Value to verify.</param>
    /// <param name="min">Minimum value to compare <paramref name="param"/> to.</param>
    /// <param name="max">Maximum value to compare <paramref name="param"/> to.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="param"/> is exclusively between <paramref name="min"/> and <paramref name="max"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsEmpty<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Any() )
            ExceptionThrower.Throw( Exceptions.NotEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsEmpty<T>(IReadOnlyCollection<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsEmpty() )
            ExceptionThrower.Throw( Exceptions.NotEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is empty.
    /// </summary>
    /// <param name="param">String to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsEmpty(string param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Length > 0 )
            ExceptionThrower.Throw( Exceptions.NotEmpty( param, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotEmpty<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.Any() )
            ExceptionThrower.Throw( Exceptions.Empty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotEmpty<T>(IReadOnlyCollection<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.IsEmpty() )
            ExceptionThrower.Throw( Exceptions.Empty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not empty.
    /// </summary>
    /// <param name="param">String to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotEmpty(string param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.Length == 0 )
            ExceptionThrower.Throw( Exceptions.Empty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is null or empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not null or empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNullOrEmpty<T>(IEnumerable<T>? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NotNullOrEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is null or empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not null or empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNullOrEmpty<T>(IReadOnlyCollection<T>? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NotNullOrEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is null or empty.
    /// </summary>
    /// <param name="param">String to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not null or empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNullOrEmpty(string? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! string.IsNullOrEmpty( param ) )
            ExceptionThrower.Throw( Exceptions.NotNullOrEmpty( param, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null or empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is null or empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrEmpty<T>([NotNull] IEnumerable<T>? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NullOrEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null or empty.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is null or empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrEmpty<T>(
        [NotNull] IReadOnlyCollection<T>? param,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( param.IsNullOrEmpty() )
            ExceptionThrower.Throw( Exceptions.NullOrEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null or empty.
    /// </summary>
    /// <param name="param">String to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is null or empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrEmpty([NotNull] string? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( string.IsNullOrEmpty( param ) )
            ExceptionThrower.Throw( Exceptions.NullOrEmpty( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is not null and does not consist only of white-space characters.
    /// </summary>
    /// <param name="param">String to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is null or consists only of white-space characters.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsNotNullOrWhiteSpace([NotNull] string? param, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( string.IsNullOrWhiteSpace( param ) )
            ExceptionThrower.Throw( Exceptions.NullOrWhiteSpace( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least one null element.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> does not contain at least one null element.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param.All( static e => e is not null ) )
            ExceptionThrower.Throw( Exceptions.NotContainsNull( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least one null element.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> does not contain at least one null element.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( param.All( static e => e.HasValue ) )
            ExceptionThrower.Throw( Exceptions.NotContainsNull( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least one null element.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> does not contain at least one null element.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> does not contain any null elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains at least one null element.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : class
    {
        if ( param.Any( static e => e is null ) )
            ExceptionThrower.Throw( Exceptions.ContainsNull( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> does not contain any null elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains at least one null element.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContainsNull<T>(IEnumerable<T?> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : struct
    {
        if ( param.Any( static e => ! e.HasValue ) )
            ExceptionThrower.Throw( Exceptions.ContainsNull( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> does not contain any null elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains at least one null element.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Minimum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains less elements than <paramref name="count"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtLeast<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtLeast( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtLeast( count, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Minimum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains less elements than <paramref name="count"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtLeast<T>(
        IReadOnlyCollection<T> param,
        int count,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtLeast( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtLeast( count, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at most <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Maximum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains more elements than <paramref name="count"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtMost<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtMost( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtMost( count, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at most <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Maximum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains more elements than <paramref name="count"/>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsAtMost<T>(
        IReadOnlyCollection<T> param,
        int count,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsAtMost( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsAtMost( count, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> does not contain between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> contains between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> does not contain between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> contains exactly <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Exact expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> contains more or less elements than <paramref name="count"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsExactly<T>(IEnumerable<T> param, int count, [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsExactly( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsExactly( count, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains exactly <paramref name="count"/> elements.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="count">Exact expected number of elements.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> contains more or less elements than <paramref name="count"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ContainsExactly<T>(
        IReadOnlyCollection<T> param,
        int count,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.ContainsExactly( count ) )
            ExceptionThrower.Throw( Exceptions.NotContainsExactly( count, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains an element equal to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="value">Value to find.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> does not contain an element equal to the provided <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Contains<T>(IEnumerable<T> param, T value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( ! param.Any( e => e.Equals( value ) ) )
            ExceptionThrower.Throw( Exceptions.NotContains( value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> contains an element equal to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="value">Value to find.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> does not contain an element equal to the provided <paramref name="value"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> does not contain an element equal to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="value">Value to find.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> contains an element equal to the provided <paramref name="value"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void NotContains<T>(IEnumerable<T> param, T value, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IEquatable<T>
    {
        if ( param.Any( e => e.Equals( value ) ) )
            ExceptionThrower.Throw( Exceptions.Contains( value, paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> does not contain an element equal to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="value">Value to find.</param>
    /// <param name="comparer">Comparer to use for equality comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">
    /// When <paramref name="param"/> contains an element equal to the provided <paramref name="value"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least one element for whom the invocation
    /// of the provided <paramref name="predicate"/> returns <b>true</b>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="predicate">Predicate invoked for elements of the collection.</param>
    /// <param name="description">Optional description of an error.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> does not contain at least one valid element.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> contains at least one element for whom the invocation
    /// of the provided <paramref name="predicate"/> returns <b>true</b>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="predicate">Predicate invoked for elements of the collection.</param>
    /// <param name="descriptionProvider">Optional provider of a description of an error.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> does not contain at least one valid element.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> only contains elements for whom the invocation
    /// of the provided <paramref name="predicate"/> returns <b>true</b>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="predicate">Predicate invoked for elements of the collection.</param>
    /// <param name="description">Optional description of an error.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains at least one invalid element.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> only contains elements for whom the invocation
    /// of the provided <paramref name="predicate"/> returns <b>true</b>.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="predicate">Predicate invoked for elements of the collection.</param>
    /// <param name="descriptionProvider">Optional provider of a description of an error.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> contains at least one invalid element.</exception>
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

    /// <summary>
    /// Ensures that <paramref name="param"/> is an ordered collection.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not ordered.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOrdered<T>(IEnumerable<T> param, [CallerArgumentExpression( "param" )] string paramName = "")
        where T : IComparable<T>
    {
        if ( ! param.IsOrdered() )
            ExceptionThrower.Throw( Exceptions.NotOrdered( paramName ) );
    }

    /// <summary>
    /// Ensures that <paramref name="param"/> is an ordered collection.
    /// </summary>
    /// <param name="param">Collection to verify.</param>
    /// <param name="comparer">Comparer to use for comparison.</param>
    /// <param name="paramName">Optional name of the parameter.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="ArgumentException">When <paramref name="param"/> is not ordered.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void IsOrdered<T>(
        IEnumerable<T> param,
        IComparer<T> comparer,
        [CallerArgumentExpression( "param" )] string paramName = "")
    {
        if ( ! param.IsOrdered( comparer ) )
            ExceptionThrower.Throw( Exceptions.NotOrdered( paramName ) );
    }

    /// <summary>
    /// Ensures that the provided <paramref name="condition"/> is <b>true</b>.
    /// </summary>
    /// <param name="condition">Condition to verify.</param>
    /// <param name="description">Optional description of the error.</param>
    /// <exception cref="ArgumentException">When <paramref name="condition"/> is <b>false</b>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void True(bool condition, [CallerArgumentExpression( "condition" )] string description = "")
    {
        if ( ! condition )
            ExceptionThrower.Throw( Exceptions.False( description ) );
    }

    /// <summary>
    /// Ensures that the provided <paramref name="condition"/> is <b>true</b>.
    /// </summary>
    /// <param name="condition">Condition to verify.</param>
    /// <param name="descriptionProvider">Optional provider of a description of the error.</param>
    /// <exception cref="ArgumentException">When <paramref name="condition"/> is <b>false</b>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void True(bool condition, Func<string>? descriptionProvider)
    {
        if ( ! condition )
            ExceptionThrower.Throw( Exceptions.False( descriptionProvider?.Invoke() ) );
    }

    /// <summary>
    /// Ensures that the provided <paramref name="condition"/> is <b>false</b>.
    /// </summary>
    /// <param name="condition">Condition to verify.</param>
    /// <param name="description">Optional description of the error.</param>
    /// <exception cref="ArgumentException">When <paramref name="condition"/> is <b>true</b>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void False(bool condition, [CallerArgumentExpression( "condition" )] string description = "")
    {
        if ( condition )
            ExceptionThrower.Throw( Exceptions.True( description ) );
    }

    /// <summary>
    /// Ensures that the provided <paramref name="condition"/> is <b>false</b>.
    /// </summary>
    /// <param name="condition">Condition to verify.</param>
    /// <param name="descriptionProvider">Optional provider of a description of the error.</param>
    /// <exception cref="ArgumentException">When <paramref name="condition"/> is <b>true</b>.</exception>
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
