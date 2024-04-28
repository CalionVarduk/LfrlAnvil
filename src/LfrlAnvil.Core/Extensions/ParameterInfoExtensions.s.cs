using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="ParameterInfo"/> extension methods.
/// </summary>
public static class ParameterInfoExtensions
{
    /// <summary>
    /// Attempts to retrieve an attribute of the specified type from the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Parameter to retrieve an attribute from.</param>
    /// <param name="attributeType">Type of an <see cref="Attribute"/> to search for.</param>
    /// <param name="inherit">
    /// Specifies whether or not to include parameter's ancestors in the search. Equal to <b>true</b> by default.
    /// </param>
    /// <returns>Found attribute's instance or null, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttribute(ParameterInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Attribute? GetAttribute(this ParameterInfo parameter, Type attributeType, bool inherit = true)
    {
        return Attribute.GetCustomAttribute( parameter, attributeType, inherit );
    }

    /// <summary>
    /// Attempts to retrieve an attribute of the specified type from the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Parameter to retrieve an attribute from.</param>
    /// <param name="inherit">
    /// Specifies whether or not to include parameter's ancestors in the search. Equal to <b>true</b> by default.
    /// </param>
    /// <typeparam name="T">Type of an <see cref="Attribute"/> to search for.</typeparam>
    /// <returns>Found attribute's instance or null, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttribute(ParameterInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? GetAttribute<T>(this ParameterInfo parameter, bool inherit = true)
        where T : Attribute
    {
        return DynamicCast.To<T>( parameter.GetAttribute( typeof( T ), inherit ) );
    }

    /// <summary>
    /// Attempts to retrieve all attributes of the specified type from the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Parameter to retrieve attributes from.</param>
    /// <param name="attributeType">Type of an <see cref="Attribute"/> to search for.</param>
    /// <param name="inherit">
    /// Specifies whether or not to include parameter's ancestors in the search. Equal to <b>true</b> by default.
    /// </param>
    /// <returns>All found attribute's instances or an empty array, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttributes(ParameterInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Attribute[] GetAttributeRange(this ParameterInfo parameter, Type attributeType, bool inherit = true)
    {
        return Attribute.GetCustomAttributes( parameter, attributeType, inherit );
    }

    /// <summary>
    /// Attempts to retrieve all attributes of the specified type from the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Parameter to retrieve attributes from.</param>
    /// <param name="inherit">
    /// Specifies whether or not to include parameter's ancestors in the search. Equal to <b>true</b> by default.
    /// </param>
    /// <typeparam name="T">Type of an <see cref="Attribute"/> to search for.</typeparam>
    /// <returns>All found attribute's instances or an empty array, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttributes(ParameterInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> GetAttributeRange<T>(this ParameterInfo parameter, bool inherit = true)
        where T : Attribute
    {
        return parameter.GetAttributeRange( typeof( T ), inherit ).Select( DynamicCast.To<T> )!;
    }

    /// <summary>
    /// Checks if an attribute of the specified type exists for the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Parameter to check.</param>
    /// <param name="attributeType">Type of an <see cref="Attribute"/> to search for.</param>
    /// <param name="inherit">
    /// Specifies whether or not to include parameter's ancestors in the search. Equal to <b>true</b> by default.
    /// </param>
    /// <returns>
    /// <b>true</b> when an attribute of the specified type exists for the given <paramref name="parameter"/>, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>See <see cref="Attribute.IsDefined(ParameterInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasAttribute(this ParameterInfo parameter, Type attributeType, bool inherit = true)
    {
        return Attribute.IsDefined( parameter, attributeType, inherit );
    }

    /// <summary>
    /// Checks if an attribute of the specified type exists for the given <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Parameter to check.</param>
    /// <param name="inherit">
    /// Specifies whether or not to include parameter's ancestors in the search. Equal to <b>true</b> by default.
    /// </param>
    /// <typeparam name="T">Type of an <see cref="Attribute"/> to search for.</typeparam>
    /// <returns>
    /// <b>true</b> when an attribute of the specified type exists for the given <paramref name="parameter"/>, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>See <see cref="Attribute.IsDefined(ParameterInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasAttribute<T>(this ParameterInfo parameter, bool inherit = true)
        where T : Attribute
    {
        return parameter.HasAttribute( typeof( T ), inherit );
    }

    /// <summary>
    /// Creates a string representation of the provided <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">Source parameter info.</param>
    /// <returns>String representation of the provided <paramref name="parameter"/>.</returns>
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
