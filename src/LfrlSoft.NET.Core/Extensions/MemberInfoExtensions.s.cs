using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Extensions
{
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
            return (T?) member.GetAttribute( typeof( T ), inherit );
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
            return member.GetAttributeRange( typeof( T ), inherit ).Select( a => (T) a );
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
            return member as EventInfo;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static FieldInfo? TryAsField(this MemberInfo member)
        {
            return member as FieldInfo;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static PropertyInfo? TryAsProperty(this MemberInfo member)
        {
            return member as PropertyInfo;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type? TryAsType(this MemberInfo member)
        {
            return member as Type;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ConstructorInfo? TryAsConstructor(this MemberInfo member)
        {
            return member as ConstructorInfo;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MethodInfo? TryAsMethod(this MemberInfo member)
        {
            return member as MethodInfo;
        }
    }
}
