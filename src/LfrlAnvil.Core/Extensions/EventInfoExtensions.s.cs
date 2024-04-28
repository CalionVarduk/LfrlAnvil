using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="EventInfo"/> extension methods.
/// </summary>
public static class EventInfoExtensions
{
    /// <summary>
    /// Creates a string representation of the provided <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Source event info.</param>
    /// <param name="includeDeclaringType">
    /// When set to <b>true</b>, then <see cref="MemberInfo.DeclaringType"/> will be included in the string. <b>false</b> by default.
    /// </param>
    /// <returns>String representation of the provided <paramref name="event"/>.</returns>
    [Pure]
    public static string GetDebugString(this EventInfo @event, bool includeDeclaringType = false)
    {
        var builder = new StringBuilder();
        if ( @event.EventHandlerType is not null )
            TypeExtensions.AppendDebugString( builder, @event.EventHandlerType ).Append( ' ' );

        if ( includeDeclaringType && @event.DeclaringType is not null )
            TypeExtensions.AppendDebugString( builder, @event.DeclaringType ).Append( '.' );

        return builder.Append( @event.Name ).Append( " [event]" ).ToString();
    }
}
