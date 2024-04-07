using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class FieldInfoExtensions
{
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
