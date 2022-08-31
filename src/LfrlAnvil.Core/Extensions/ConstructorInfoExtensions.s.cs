using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class ConstructorInfoExtensions
{
    [Pure]
    public static string GetDebugString(this ConstructorInfo ctor, bool includeDeclaringType = false)
    {
        var builder = new StringBuilder();
        if ( includeDeclaringType && ctor.DeclaringType is not null )
            TypeExtensions.AppendDebugString( builder, ctor.DeclaringType ).Append( '.' );

        builder.Append( ctor.Name );
        return MethodInfoExtensions.AppendParametersString( builder, ctor.GetParameters() ).ToString();
    }
}
