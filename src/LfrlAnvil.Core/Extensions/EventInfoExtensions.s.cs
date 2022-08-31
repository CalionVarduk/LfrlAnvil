using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class EventInfoExtensions
{
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
