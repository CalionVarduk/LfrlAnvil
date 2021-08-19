using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ParameterInfoExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Attribute? GetAttribute(this ParameterInfo parameter, Type attributeType, bool inherit = true)
        {
            return Attribute.GetCustomAttribute( parameter, attributeType, inherit );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T? GetAttribute<T>(this ParameterInfo parameter, bool inherit = true)
            where T : Attribute
        {
            return (T?) parameter.GetAttribute( typeof( T ), inherit );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Attribute[] GetAttributeRange(this ParameterInfo parameter, Type attributeType, bool inherit = true)
        {
            return Attribute.GetCustomAttributes( parameter, attributeType, inherit );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> GetAttributeRange<T>(this ParameterInfo parameter, bool inherit = true)
            where T : Attribute
        {
            return parameter.GetAttributeRange( typeof( T ), inherit ).Select( a => (T) a );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasAttribute(this ParameterInfo parameter, Type attributeType, bool inherit = true)
        {
            return Attribute.IsDefined( parameter, attributeType, inherit );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasAttribute<T>(this ParameterInfo parameter, bool inherit = true)
            where T : Attribute
        {
            return parameter.HasAttribute( typeof( T ), inherit );
        }
    }
}
