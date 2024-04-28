using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="FieldInfo"/> extension methods.
/// </summary>
public static class FieldInfoExtensions
{
    /// <summary>
    /// Attempts to get the auto-property backed by the provided compiler-generated <paramref name="source"/> field.
    /// </summary>
    /// <param name="source">Source field.</param>
    /// <returns>
    /// <see cref="PropertyInfo"/> instance backed by the <paramref name="source"/> field, if it exists, otherwise <b>null</b>.
    /// </returns>
    /// <remarks>Backing field names are of the form <i>&lt;{PROPERTY_NAME}&gt;k__BackingField</i>.</remarks>
    [Pure]
    public static PropertyInfo? GetBackedProperty(this FieldInfo source)
    {
        if ( ! source.IsPrivate
            || ! source.Name.StartsWith( '<' )
            || ! Attribute.IsDefined( source, typeof( CompilerGeneratedAttribute ) ) )
            return null;

        var nameEndIndex = Math.Max( source.Name.LastIndexOf( ">k__BackingField", StringComparison.Ordinal ), 1 );
        var propertyName = source.Name.Substring( startIndex: 1, length: nameEndIndex - 1 );

        var result = source.DeclaringType?
            .GetProperty( propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

        return result;
    }

    /// <summary>
    /// Creates a string representation of the provided <paramref name="field"/>.
    /// </summary>
    /// <param name="field">Source field info.</param>
    /// <param name="includeDeclaringType">
    /// When set to <b>true</b>, then <see cref="MemberInfo.DeclaringType"/> will be included in the string. <b>false</b> by default.
    /// </param>
    /// <returns>String representation of the provided <paramref name="field"/>.</returns>
    [Pure]
    public static string GetDebugString(this FieldInfo field, bool includeDeclaringType = false)
    {
        var builder = new StringBuilder();
        TypeExtensions.AppendDebugString( builder, field.FieldType ).Append( ' ' );

        if ( includeDeclaringType && field.DeclaringType is not null )
            TypeExtensions.AppendDebugString( builder, field.DeclaringType ).Append( '.' );

        return builder.Append( field.Name ).ToString();
    }
}
