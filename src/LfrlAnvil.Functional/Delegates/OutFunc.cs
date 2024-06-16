// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
