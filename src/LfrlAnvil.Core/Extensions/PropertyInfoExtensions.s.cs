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
/// Contains <see cref="PropertyInfo"/> extension methods.
/// </summary>
public static class PropertyInfoExtensions
{
    /// <summary>
    /// Attempts to get the compiler-generated backing field for the given <paramref name="source"/> property.
    /// </summary>
    /// <param name="source">Source property.</param>
    /// <returns><see cref="FieldInfo"/> instance that is the backing field for the given property, if it exists, otherwise null.</returns>
    /// <remarks>Backing field names are of the form <i>&lt;{PROPERTY_NAME}&gt;k__BackingField</i>.</remarks>
    [Pure]
    public static FieldInfo? GetBackingField(this PropertyInfo source)
    {
        var backingFieldName = $"<{source.Name}>k__BackingField";

        var result = source.DeclaringType?
            .GetField( backingFieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic );

        return result;
    }

    /// <summary>
    /// Checks whether or not the given <paramref name="property"/> is an indexer.
    /// </summary>
    /// <param name="property">Property to check.</param>
    /// <returns><b>true</b> when <paramref name="property"/> is an indexer, otherwise <b>false</b>.</returns>
    /// <remarks>See <see cref="PropertyInfo.GetIndexParameters()"/> for more information.</remarks>
    [Pure]
    public static bool IsIndexer(this PropertyInfo property)
    {
        return property.GetIndexParameters().Length > 0;
    }

    /// <summary>
    /// Creates a string representation of the provided <paramref name="property"/>.
    /// </summary>
    /// <param name="property">Source property info.</param>
    /// <param name="includeDeclaringType">
    /// When set to <b>true</b>, then <see cref="MemberInfo.DeclaringType"/> will be included in the string. <b>false</b> by default.
    /// </param>
    /// <returns>String representation of the provided <paramref name="property"/>.</returns>
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
