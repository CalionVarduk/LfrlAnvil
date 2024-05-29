using System;

namespace LfrlAnvil.Functional.Delegates;

/// <summary>
/// Represents a delegate that returns <see cref="Boolean"/> and has a single <b>out</b> parameter of <typeparamref name="TResult"/> type.
/// </summary>
/// <typeparam name="TResult"><b>out</b> parameter type.</typeparam>
public delegate bool OutFunc<TResult>(out TResult result);

/// <summary>
/// Represents a delegate that returns <see cref="Boolean"/> and has two parameters
/// with the last one being an <b>out</b> parameter of <typeparamref name="TResult"/> type.
/// </summary>
/// <typeparam name="T1">Type of the first parameter.</typeparam>
/// <typeparam name="TResult"><b>out</b> parameter type.</typeparam>
public delegate bool OutFunc<in T1, TResult>(T1 t1, out TResult result);

/// <summary>
/// Represents a delegate that returns <see cref="Boolean"/> and has three parameters
/// with the last one being an <b>out</b> parameter of <typeparamref name="TResult"/> type.
/// </summary>
/// <typeparam name="T1">Type of the first parameter.</typeparam>
/// <typeparam name="T2">Type of the second parameter.</typeparam>
/// <typeparam name="TResult"><b>out</b> parameter type.</typeparam>
public delegate bool OutFunc<in T1, in T2, TResult>(T1 t1, T2 t2, out TResult result);

/// <summary>
/// Represents a delegate that returns <see cref="Boolean"/> and has four parameters
/// with the last one being an <b>out</b> parameter of <typeparamref name="TResult"/> type.
/// </summary>
/// <typeparam name="T1">Type of the first parameter.</typeparam>
/// <typeparam name="T2">Type of the second parameter.</typeparam>
/// <typeparam name="T3">Type of the third parameter.</typeparam>
/// <typeparam name="TResult"><b>out</b> parameter type.</typeparam>
public delegate bool OutFunc<in T1, in T2, in T3, TResult>(T1 t1, T2 t2, T3 t3, out TResult result);
