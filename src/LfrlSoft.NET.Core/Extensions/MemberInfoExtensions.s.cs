using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class MemberInfoExtensions
    {
        public static Attribute? GetAttribute(this MemberInfo member, Type attributeType, bool inherit = true)
        {
            return Attribute.GetCustomAttribute( member, attributeType, inherit );
        }

        public static T? GetAttribute<T>(this MemberInfo member, bool inherit = true)
            where T : Attribute
        {
            return (T?) member.GetAttribute( typeof( T ), inherit );
        }

        public static Attribute[] GetAttributeRange(this MemberInfo member, Type attributeType, bool inherit = true)
        {
            return Attribute.GetCustomAttributes( member, attributeType, inherit );
        }

        public static IEnumerable<T> GetAttributeRange<T>(this MemberInfo member, bool inherit = true)
            where T : Attribute
        {
            return member.GetAttributeRange( typeof( T ), inherit ).Select( a => (T) a );
        }

        public static bool HasAttribute(this MemberInfo member, Type attributeType, bool inherit = true)
        {
            return Attribute.IsDefined( member, attributeType, inherit );
        }

        public static bool HasAttribute<T>(this MemberInfo member, bool inherit = true)
            where T : Attribute
        {
            return member.HasAttribute( typeof( T ), inherit );
        }

        public static EventInfo? TryAsEvent(this MemberInfo member)
        {
            return member as EventInfo;
        }

        public static FieldInfo? TryAsField(this MemberInfo member)
        {
            return member as FieldInfo;
        }

        public static PropertyInfo? TryAsProperty(this MemberInfo member)
        {
            return member as PropertyInfo;
        }

        public static Type? TryAsType(this MemberInfo member)
        {
            return member as Type;
        }

        public static ConstructorInfo? TryAsConstructor(this MemberInfo member)
        {
            return member as ConstructorInfo;
        }

        public static MethodInfo? TryAsMethod(this MemberInfo member)
        {
            return member as MethodInfo;
        }
    }
}
