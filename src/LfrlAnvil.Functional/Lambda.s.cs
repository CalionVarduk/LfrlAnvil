using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional.Delegates;

namespace LfrlAnvil.Functional;

/// <summary>
/// Contains generic delegate members.
/// </summary>
/// <typeparam name="T"></typeparam>
public static class Lambda<T>
{
    /// <summary>
    /// Represents an identity function, that is a function that accepts a single parameter and returns that parameter.
    /// </summary>
    public static readonly Func<T, T> Identity = static x => x;
}

/// <summary>
/// Contains various methods related to delegates and expression trees.
/// </summary>
public static class Lambda
{
    /// <summary>
    /// Represents an <see cref="Action"/> that does nothing.
    /// </summary>
    public static readonly Action NoOp = static () => { };

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<TReturn> Of<TReturn>(Func<TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, TReturn> Of<T1, TReturn>(Func<T1, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, TReturn> Of<T1, T2, TReturn>(Func<T1, T2, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, TReturn> Of<T1, T2, T3, TReturn>(Func<T1, T2, T3, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, TReturn> Of<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, T5, TReturn> Of<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, T5, T6, TReturn> Of<T1, T2, T3, T4, T5, T6, TReturn>(Func<T1, T2, T3, T4, T5, T6, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <typeparam name="T7">Delegate's seventh parameter's type.</typeparam>
    /// <typeparam name="TReturn">Delegate's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, T5, T6, T7, TReturn> Of<T1, T2, T3, T4, T5, T6, T7, TReturn>(
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action Of(Action action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1> Of<T1>(Action<T1> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2> Of<T1, T2>(Action<T1, T2> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3> Of<T1, T2, T3>(Action<T1, T2, T3> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4> Of<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4, T5> Of<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4, T5, T6> Of<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <typeparam name="T7">Delegate's seventh parameter's type.</typeparam>
    /// <returns><paramref name="action"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4, T5, T6, T7> Of<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
    {
        return action;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's second <b>out</b> parameter's type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OutFunc<T1> Of<T1>(OutFunc<T1> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second <b>out</b> parameter's type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OutFunc<T1, T2> Of<T1, T2>(OutFunc<T1, T2> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third <b>out</b> parameter's type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OutFunc<T1, T2, T3> Of<T1, T2, T3>(OutFunc<T1, T2, T3> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to return.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth <b>out</b> parameter's type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OutFunc<T1, T2, T3, T4> Of<T1, T2, T3, T4>(OutFunc<T1, T2, T3, T4> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<TReturn>> ExpressionOf<TReturn>(Expression<Func<TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, TReturn>> ExpressionOf<T1, TReturn>(Expression<Func<T1, TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="T2">Expression's second parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, T2, TReturn>> ExpressionOf<T1, T2, TReturn>(Expression<Func<T1, T2, TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="T2">Expression's second parameter's type.</typeparam>
    /// <typeparam name="T3">Expression's third parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, T2, T3, TReturn>> ExpressionOf<T1, T2, T3, TReturn>(Expression<Func<T1, T2, T3, TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="T2">Expression's second parameter's type.</typeparam>
    /// <typeparam name="T3">Expression's third parameter's type.</typeparam>
    /// <typeparam name="T4">Expression's fourth parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, T2, T3, T4, TReturn>> ExpressionOf<T1, T2, T3, T4, TReturn>(
        Expression<Func<T1, T2, T3, T4, TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="T2">Expression's second parameter's type.</typeparam>
    /// <typeparam name="T3">Expression's third parameter's type.</typeparam>
    /// <typeparam name="T4">Expression's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Expression's fifth parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, T2, T3, T4, T5, TReturn>> ExpressionOf<T1, T2, T3, T4, T5, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="T2">Expression's second parameter's type.</typeparam>
    /// <typeparam name="T3">Expression's third parameter's type.</typeparam>
    /// <typeparam name="T4">Expression's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Expression's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Expression's sixth parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> ExpressionOf<T1, T2, T3, T4, T5, T6, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> func)
    {
        return func;
    }

    /// <summary>
    /// Returns the provided expression <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Expression to return.</param>
    /// <typeparam name="T1">Expression's first parameter's type.</typeparam>
    /// <typeparam name="T2">Expression's second parameter's type.</typeparam>
    /// <typeparam name="T3">Expression's third parameter's type.</typeparam>
    /// <typeparam name="T4">Expression's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Expression's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Expression's sixth parameter's type.</typeparam>
    /// <typeparam name="T7">Expression's seventh parameter's type.</typeparam>
    /// <typeparam name="TReturn">Expression's return type.</typeparam>
    /// <returns><paramref name="func"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> ExpressionOf<T1, T2, T3, T4, T5, T6, T7, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> func)
    {
        return func;
    }
}
