using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ParameterInfoExtensions
    {
        [Pure]
        public static Attribute? GetAttribute(this ParameterInfo parameter, Type attributeType, bool inherit = true)
        {
            return Attribute.GetCustomAttribute( parameter, attributeType, inherit );
        }

        [Pure]
        public static T? GetAttribute<T>(this ParameterInfo parameter, bool inherit = true)
            where T : Attribute
        {
            return (T?) parameter.GetAttribute( typeof( T ), inherit );
        }

        [Pure]
        public static Attribute[] GetAttributeRange(this ParameterInfo parameter, Type attributeType, bool inherit = true)
        {
            return Attribute.GetCustomAttributes( parameter, attributeType, inherit );
        }

        [Pure]
        public static IEnumerable<T> GetAttributeRange<T>(this ParameterInfo parameter, bool inherit = true)
            where T : Attribute
        {
            return parameter.GetAttributeRange( typeof( T ), inherit ).Select( a => (T) a );
        }

        [Pure]
        public static bool HasAttribute(this ParameterInfo parameter, Type attributeType, bool inherit = true)
        {
            return Attribute.IsDefined( parameter, attributeType, inherit );
        }

        [Pure]
        public static bool HasAttribute<T>(this ParameterInfo parameter, bool inherit = true)
            where T : Attribute
        {
            return parameter.HasAttribute( typeof( T ), inherit );
        }
    }
}
