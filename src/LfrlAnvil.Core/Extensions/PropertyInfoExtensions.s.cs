using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class PropertyInfoExtensions
{
    [Pure]
    public static FieldInfo? GetBackingField(this PropertyInfo source)
    {
        var backingFieldName = $"<{source.Name}>k__BackingField";

        var result = source.DeclaringType?
            .GetField( backingFieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic );

        return result;
    }

    [Pure]
    public static string GetDebugString(this PropertyInfo property, bool includeDeclaringType = false)
    {
        var builder = new StringBuilder();
        TypeExtensions.AppendDebugString( builder, property.PropertyType ).Append( ' ' );

        if ( includeDeclaringType && property.DeclaringType is not null )
            TypeExtensions.AppendDebugString( builder, property.DeclaringType ).Append( '.' );

        builder.Append( property.Name ).Append( ' ' );

        if ( property.CanRead )
            builder.Append( "[get]" );

        if ( property.CanWrite )
            builder.Append( "[set]" );

        return builder.ToString();
    }
}
