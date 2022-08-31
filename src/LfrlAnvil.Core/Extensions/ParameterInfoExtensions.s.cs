using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

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
        return DynamicCast.To<T>( parameter.GetAttribute( typeof( T ), inherit ) );
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
        return parameter.GetAttributeRange( typeof( T ), inherit ).Select( DynamicCast.To<T> )!;
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

    [Pure]
    public static string GetDebugString(this ParameterInfo parameter)
    {
        return AppendDebugString( new StringBuilder(), parameter ).ToString();
    }

    internal static StringBuilder AppendDebugString(StringBuilder builder, ParameterInfo parameter)
    {
        TypeExtensions.AppendDebugString( builder, parameter.ParameterType ).Append( ' ' ).Append( parameter.Name );

        if ( parameter.IsIn )
            builder.Append( " [in]" );
        else if ( parameter.IsOut )
            builder.Append( " [out]" );

        return builder;
    }
}
