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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// Represents a generic boxed value.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public sealed class Ref<T> : IReadOnlyRef<T>
{
    /// <summary>
    /// Creates a new <see cref="Ref{T}"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public Ref(T value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public T Value { get; set; }

    /// <inheritdoc />
    public int Count => 1;

    /// <inheritdoc />
    public T this[int index]
    {
        get
        {
            if ( index != 0 )
                throw new IndexOutOfRangeException( ExceptionResources.ExpectedIndexToBeZero );

            return Value;
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Ref{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Ref )}({Value})";
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this ref.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Value );
    }

    /// <summary>
    /// Converts provided <paramref name="obj"/> to the underlying value type.
    /// </summary>
    /// <param name="obj">Object to convert.</param>
    /// <returns><see cref="Value"/> from the <paramref name="obj"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator T(Ref<T> obj)
    {
        return obj.Value;
    }

    /// <summary>
    /// Converts provided <paramref name="value"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="value">Object to convert.</param>
    /// <returns>New <see cref="Ref{T}"/> instance.</returns>
    [Pure]
    public static explicit operator Ref<T>(T value)
    {
        return new Ref<T>( value );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="Ref{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private bool _read;

        internal Enumerator(T value)
        {
            Current = value;
            _read = false;
        }

        /// <inheritdoc />
        public T Current { get; }

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            if ( _read )
                return false;

            _read = true;
            return true;
        }

        /// <inheritdoc />
        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _read = false;
        }
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
