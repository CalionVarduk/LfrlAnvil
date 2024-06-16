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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

/// <summary>
/// Represents a generic (name, value) tuple that can be used to construct more complex <see cref="Enum"/>-like objects.
/// </summary>
/// <typeparam name="T">Enumeration type. Use the "Curiously Recurring Template Pattern" (CRTP) approach.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public abstract class Enumeration<T, TValue> : IEquatable<T>, IComparable<T>, IComparable
    where T : Enumeration<T, TValue>
    where TValue : notnull
{
    /// <summary>
    /// Creates a new <see cref="Enumeration{T,TValue}"/> instance.
    /// </summary>
    /// <param name="name">Entry's name.</param>
    /// <param name="value">Entry's value.</param>
    protected Enumeration(string name, TValue value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Enumeration entry's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Enumeration entry's value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="Enumeration{T,TValue}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"'{Name}' ({Value})";
    }

    /// <inheritdoc />
    [Pure]
    public sealed override int GetHashCode()
    {
        return EqualityComparer<TValue>.Default.GetHashCode( Value );
    }

    /// <inheritdoc />
    [Pure]
    public sealed override bool Equals(object? obj)
    {
        return obj is T e && Equals( e );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is T e ? CompareTo( e ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(T? other)
    {
        return EqualsBase( other );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(T? other)
    {
        return CompareToBase( other );
    }

    /// <summary>
    /// Converts this instance to the underlying value type. Returns <see cref="Value"/> of <paramref name="e"/>.
    /// </summary>
    /// <param name="e">Enumeration entry.</param>
    /// <returns><see cref="Value"/> of <paramref name="e"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TValue(Enumeration<T, TValue> e)
    {
        return e.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a?.EqualsBase( b ) ?? b is null;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return ! (a?.EqualsBase( b ) ?? b is null);
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >=(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is null ? b is null : a.CompareToBase( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is null ? b is not null : a.CompareToBase( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <=(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is null || a.CompareToBase( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is not null && a.CompareToBase( b ) > 0;
    }

    /// <summary>
    /// Extracts all valid public and static enumeration members (limited to auto-properties and members)
    /// of <typeparamref name="T"/> type from that type and creates a dictionary out of them where entries are identified by
    /// their underlying <see cref="Value"/>.
    /// </summary>
    /// <returns>New <see cref="Dictionary{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When underlying enumeration values are duplicated.</exception>
    [Pure]
    protected static Dictionary<TValue, T> GetValueDictionary()
    {
        return GetAllMembers().ToDictionary( static e => e.Value );
    }

    /// <summary>
    /// Extracts all valid public and static enumeration members (limited to auto-properties and members)
    /// of <typeparamref name="T"/> type from that type and creates a dictionary out of them where entries are identified by
    /// their underlying <see cref="Name"/>.
    /// </summary>
    /// <returns>New <see cref="Dictionary{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When underlying enumeration names are duplicated.</exception>
    [Pure]
    protected static Dictionary<string, T> GetNameDictionary()
    {
        return GetAllMembers().ToDictionary( static e => e.Name );
    }

    /// <summary>
    /// Extracts all valid public and static enumeration members (limited to auto-properties and members)
    /// of <typeparamref name="T"/> type from that type.
    /// </summary>
    /// <returns>Collection of valid members of <typeparamref name="T"/> type from that type.</returns>
    [Pure]
    protected static IEnumerable<T> GetAllMembers()
    {
        var members = typeof( T ).GetMembers( BindingFlags.Public | BindingFlags.Static );
        foreach ( var member in members )
        {
            T? entry = null;
            if ( member is PropertyInfo property )
            {
                if ( property.PropertyType != typeof( T ) || property.GetBackingField() is null )
                    continue;

                entry = property.GetValue( null ) as T;
            }
            else if ( member is FieldInfo field )
            {
                if ( field.FieldType != typeof( T ) )
                    continue;

                entry = field.GetValue( null ) as T;
            }

            if ( entry is not null )
                yield return entry;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool EqualsBase(Enumeration<T, TValue>? other)
    {
        return other is not null && EqualityComparer<TValue>.Default.Equals( Value, other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int CompareToBase(Enumeration<T, TValue>? other)
    {
        return other is not null ? Comparer<TValue>.Default.Compare( Value, other.Value ) : 1;
    }
}
