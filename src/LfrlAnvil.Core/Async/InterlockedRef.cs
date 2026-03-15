// Copyright 2026 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight representation of an <see cref="Interlocked"/> (atomic) ref type.
/// </summary>
/// <typeparam name="T">Ref type.</typeparam>
public struct InterlockedRef<T> : IEquatable<InterlockedRef<T>>
    where T : class?
{
    private T _value;

    /// <summary>
    /// Creates a new <see cref="InterlockedRef{T}"/> instance.
    /// </summary>
    /// <param name="value">Initial value.</param>
    public InterlockedRef(T value)
    {
        _value = value;
    }

    /// <summary>
    /// Current value.
    /// </summary>
    public T Value => Volatile.Read( ref _value );

    /// <summary>
    /// Returns a string representation of this <see cref="InterlockedRef{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is InterlockedRef<T> b && Equals( b );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(InterlockedRef<T> other)
    {
        return ReferenceEquals( Value, other.Value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/> and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Exchange(T value)
    {
        return Interlocked.Exchange( ref _value, value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="comparand"/>
    /// and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="comparand">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T CompareExchange(T value, T comparand)
    {
        return Interlocked.CompareExchange( ref _value!, value, comparand );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(T value)
    {
        return ! ReferenceEquals( Exchange( value ), value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="expected"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="expected">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(T value, T expected)
    {
        var oldValue = CompareExchange( value, expected );
        return ! ReferenceEquals( oldValue, value ) && ReferenceEquals( oldValue, expected );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedRef<T> a, InterlockedRef<T> b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(InterlockedRef<T> a, InterlockedRef<T> b)
    {
        return ! a.Equals( b );
    }
}
