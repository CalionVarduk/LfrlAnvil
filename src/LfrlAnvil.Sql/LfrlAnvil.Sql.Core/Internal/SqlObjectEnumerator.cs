﻿// Copyright 2024 Łukasz Furlepa
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a lightweight enumerator of a collection of <see cref="SqlObject"/> instances.
/// </summary>
/// <typeparam name="T">SQL object type.</typeparam>
public struct SqlObjectEnumerator<T> : IEnumerator<T>
    where T : SqlObject
{
    private Dictionary<string, T>.ValueCollection.Enumerator _enumerator;

    internal SqlObjectEnumerator(Dictionary<string, T> source)
    {
        _enumerator = source.Values.GetEnumerator();
    }

    /// <inheritdoc />
    public T Current => _enumerator.Current;

    object IEnumerator.Current => Current;

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{T,TDestination}"/> instance.
    /// </summary>
    /// <typeparam name="TDestination">Destination SQL object type.</typeparam>
    /// <returns>New <see cref="SqlObjectEnumerator{T,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectEnumerator<T, TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : T
    {
        return new SqlObjectEnumerator<T, TDestination>( _enumerator );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _enumerator.Dispose();
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    void IEnumerator.Reset()
    {
        (( IEnumerator )_enumerator).Reset();
    }
}

/// <summary>
/// Represents a lightweight enumerator of a collection of <see cref="SqlObject"/> instances.
/// </summary>
/// <typeparam name="TSource">Source SQL object type.</typeparam>
/// <typeparam name="TDestination">Destination SQL object type.</typeparam>
public struct SqlObjectEnumerator<TSource, TDestination> : IEnumerator<TDestination>
    where TSource : SqlObject
    where TDestination : TSource
{
    private Dictionary<string, TSource>.ValueCollection.Enumerator _enumerator;

    internal SqlObjectEnumerator(Dictionary<string, TSource>.ValueCollection.Enumerator enumerator)
    {
        _enumerator = enumerator;
    }

    /// <inheritdoc />
    public TDestination Current => ReinterpretCast.To<TDestination>( _enumerator.Current );

    object IEnumerator.Current => Current;

    /// <inheritdoc />
    public void Dispose()
    {
        _enumerator.Dispose();
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    void IEnumerator.Reset()
    {
        (( IEnumerator )_enumerator).Reset();
    }
}
