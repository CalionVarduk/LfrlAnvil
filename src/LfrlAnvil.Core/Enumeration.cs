using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

public abstract class Enumeration<T, TValue> : IEquatable<T>, IComparable<T>, IComparable
    where T : Enumeration<T, TValue>
    where TValue : notnull
{
    protected Enumeration(string name, TValue value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public TValue Value { get; }

    [Pure]
    public override string ToString()
    {
        return $"'{Name}' ({Value})";
    }

    [Pure]
    public sealed override int GetHashCode()
    {
        return EqualityComparer<TValue>.Default.GetHashCode( Value );
    }

    [Pure]
    public sealed override bool Equals(object? obj)
    {
        return obj is T e && Equals( e );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is T e ? CompareTo( e ) : 1;
    }

    [Pure]
    public bool Equals(T? other)
    {
        return EqualsBase( other );
    }

    [Pure]
    public int CompareTo(T? other)
    {
        return CompareToBase( other );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TValue(Enumeration<T, TValue> e)
    {
        return e.Value;
    }

    [Pure]
    public static bool operator ==(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a?.EqualsBase( b ) ?? b is null;
    }

    [Pure]
    public static bool operator !=(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return ! (a?.EqualsBase( b ) ?? b is null);
    }

    [Pure]
    public static bool operator >=(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is null ? b is null : a.CompareToBase( b ) >= 0;
    }

    [Pure]
    public static bool operator <(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is null ? b is not null : a.CompareToBase( b ) < 0;
    }

    [Pure]
    public static bool operator <=(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is null || a.CompareToBase( b ) <= 0;
    }

    [Pure]
    public static bool operator >(Enumeration<T, TValue>? a, Enumeration<T, TValue>? b)
    {
        return a is not null && a.CompareToBase( b ) > 0;
    }

    [Pure]
    protected static IReadOnlyDictionary<TValue, T> GetValueDictionary()
    {
        return GetAllMembers().ToDictionary( e => e.Value );
    }

    [Pure]
    protected static IReadOnlyDictionary<string, T> GetNameDictionary()
    {
        return GetAllMembers().ToDictionary( e => e.Name );
    }

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
