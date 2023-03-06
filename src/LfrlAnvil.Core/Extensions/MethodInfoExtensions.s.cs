using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class MethodInfoExtensions
{
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
            builder.Length -= 2;

        builder.Append( close );
        return builder;
    }
}
