using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="ConstructorInfo"/> extension methods.
/// </summary>
public static class ConstructorInfoExtensions
{
    /// <summary>
    /// Creates a string representation of the provided <paramref name="ctor"/>.
    /// </summary>
    /// <param name="ctor">Source constructor info.</param>
    /// <param name="includeDeclaringType">
    /// When set to <b>true</b>, then <see cref="MemberInfo.DeclaringType"/> will be included in the string. <b>false</b> by default.
    /// </param>
    /// <returns>String representation of the provided <paramref name="ctor"/>.</returns>
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
