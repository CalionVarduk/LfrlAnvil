using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class FieldInfoExtensions
{
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
