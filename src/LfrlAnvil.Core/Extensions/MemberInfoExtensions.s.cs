using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class MemberInfoExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Attribute? GetAttribute(this MemberInfo member, Type attributeType, bool inherit = true)
    {
        return Attribute.GetCustomAttribute( member, attributeType, inherit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? GetAttribute<T>(this MemberInfo member, bool inherit = true)
        where T : Attribute
    {
        return DynamicCast.To<T>( member.GetAttribute( typeof( T ), inherit ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Attribute[] GetAttributeRange(this MemberInfo member, Type attributeType, bool inherit = true)
    {
        return Attribute.GetCustomAttributes( member, attributeType, inherit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> GetAttributeRange<T>(this MemberInfo member, bool inherit = true)
        where T : Attribute
    {
        return member.GetAttributeRange( typeof( T ), inherit ).Select( DynamicCast.To<T> )!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasAttribute(this MemberInfo member, Type attributeType, bool inherit = true)
    {
        return Attribute.IsDefined( member, attributeType, inherit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasAttribute<T>(this MemberInfo member, bool inherit = true)
        where T : Attribute
    {
        return member.HasAttribute( typeof( T ), inherit );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EventInfo? TryAsEvent(this MemberInfo member)
    {
        return DynamicCast.TryTo<EventInfo>( member );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FieldInfo? TryAsField(this MemberInfo member)
    {
        return DynamicCast.TryTo<FieldInfo>( member );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PropertyInfo? TryAsProperty(this MemberInfo member)
    {
        return DynamicCast.TryTo<PropertyInfo>( member );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? TryAsType(this MemberInfo member)
    {
        return DynamicCast.TryTo<Type>( member );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ConstructorInfo? TryAsConstructor(this MemberInfo member)
    {
        return DynamicCast.TryTo<ConstructorInfo>( member );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MethodInfo? TryAsMethod(this MemberInfo member)
    {
        return DynamicCast.TryTo<MethodInfo>( member );
    }
}
