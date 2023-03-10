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
    public static bool IsIndexer(this PropertyInfo property)
    {
        return property.GetIndexParameters().Length > 0;
    }

    [Pure]
    public static string GetDebugString(this PropertyInfo property, bool includeDeclaringType = false)
    {
        var builder = new StringBuilder();
        TypeExtensions.AppendDebugString( builder, property.PropertyType ).Append( ' ' );

        if ( includeDeclaringType && property.DeclaringType is not null )
            TypeExtensions.AppendDebugString( builder, property.DeclaringType ).Append( '.' );

        builder.Append( property.Name );

        var indexParameters = property.GetIndexParameters();
        if ( indexParameters.Length > 0 )
            MethodInfoExtensions.AppendParametersString( builder, indexParameters, '[', ']' );

        builder.Append( ' ' );

        if ( property.CanRead )
            builder.Append( "[get]" );

        if ( property.CanWrite )
            builder.Append( "[set]" );

        return builder.ToString();
    }
}
