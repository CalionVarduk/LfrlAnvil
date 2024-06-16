// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
