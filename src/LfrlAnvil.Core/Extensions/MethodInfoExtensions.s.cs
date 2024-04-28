using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="MethodInfo"/> extension methods.
/// </summary>
public static class MethodInfoExtensions
{
    /// <summary>
    /// Creates a string representation of the provided <paramref name="method"/>.
    /// </summary>
    /// <param name="method">Source method info.</param>
    /// <param name="includeDeclaringType">
    /// When set to <b>true</b>, then <see cref="MemberInfo.DeclaringType"/> will be included in the string. <b>false</b> by default.
    /// </param>
    /// <returns>String representation of the provided <paramref name="method"/>.</returns>
    [Pure]
    public static string GetDebugString(this MethodInfo method, bool includeDeclaringType = false)
    {
        var builder = new StringBuilder();
        if ( ! method.IsGenericMethod )
        {
            TypeExtensions.AppendDebugString( builder, method.ReturnType ).Append( ' ' );

            if ( includeDeclaringType && method.DeclaringType is not null )
                TypeExtensions.AppendDebugString( builder, method.DeclaringType ).Append( '.' );

            builder.Append( method.Name );
            return AppendParametersString( builder, method.GetParameters() ).ToString();
        }

        Type[] openGenericArgs;
        var closedGenericArgs = method.GetGenericArguments();

        if ( method.IsGenericMethodDefinition )
        {
            openGenericArgs = closedGenericArgs;
            closedGenericArgs = Type.EmptyTypes;
        }
        else
        {
            method = method.GetGenericMethodDefinition();
            openGenericArgs = method.GetGenericArguments();
        }

        TypeExtensions.AppendDebugString( builder, method.ReturnType ).Append( ' ' );

        if ( includeDeclaringType && method.DeclaringType is not null )
            TypeExtensions.AppendDebugString( builder, method.DeclaringType ).Append( '.' );

        builder.Append( method.Name ).Append( '`' ).Append( openGenericArgs.Length );
        TypeExtensions.AppendGenericArgumentsString( builder, openGenericArgs, closedGenericArgs );
        return AppendParametersString( builder, method.GetParameters() ).ToString();
    }

    internal static StringBuilder AppendParametersString(
        StringBuilder builder,
        ParameterInfo[] parameters,
        char open = '(',
        char close = ')')
    {
        builder.Append( open );

        foreach ( var parameter in parameters )
            ParameterInfoExtensions.AppendDebugString( builder, parameter ).Append( ", " );

        if ( parameters.Length > 0 )
            builder.ShrinkBy( 2 );

        builder.Append( close );
        return builder;
    }
}
