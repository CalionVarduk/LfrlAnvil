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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="MemberInfo"/> extension methods.
/// </summary>
public static class MemberInfoExtensions
{
    /// <summary>
    /// Attempts to retrieve an attribute of the specified type from the given <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to retrieve an attribute from.</param>
    /// <param name="attributeType">Type of an <see cref="Attribute"/> to search for.</param>
    /// <param name="inherit">Specifies whether or not to include member's ancestors in the search. Equal to <b>true</b> by default.</param>
    /// <returns>Found attribute's instance or null, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttribute(MemberInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Attribute? GetAttribute(this MemberInfo member, Type attributeType, bool inherit = true)
    {
        return Attribute.GetCustomAttribute( member, attributeType, inherit );
    }

    /// <summary>
    /// Attempts to retrieve an attribute of the specified type from the given <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to retrieve an attribute from.</param>
    /// <param name="inherit">Specifies whether or not to include member's ancestors in the search. Equal to <b>true</b> by default.</param>
    /// <typeparam name="T">Type of an <see cref="Attribute"/> to search for.</typeparam>
    /// <returns>Found attribute's instance or null, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttribute(MemberInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? GetAttribute<T>(this MemberInfo member, bool inherit = true)
        where T : Attribute
    {
        return DynamicCast.To<T>( member.GetAttribute( typeof( T ), inherit ) );
    }

    /// <summary>
    /// Attempts to retrieve all attributes of the specified type from the given <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to retrieve attributes from.</param>
    /// <param name="attributeType">Type of an <see cref="Attribute"/> to search for.</param>
    /// <param name="inherit">Specifies whether or not to include member's ancestors in the search. Equal to <b>true</b> by default.</param>
    /// <returns>All found attribute's instances or an empty array, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttributes(MemberInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Attribute[] GetAttributeRange(this MemberInfo member, Type attributeType, bool inherit = true)
    {
        return Attribute.GetCustomAttributes( member, attributeType, inherit );
    }

    /// <summary>
    /// Attempts to retrieve all attributes of the specified type from the given <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to retrieve attributes from.</param>
    /// <param name="inherit">Specifies whether or not to include member's ancestors in the search. Equal to <b>true</b> by default.</param>
    /// <typeparam name="T">Type of an <see cref="Attribute"/> to search for.</typeparam>
    /// <returns>All found attribute's instances or an empty array, if it does not exist.</returns>
    /// <remarks>See <see cref="Attribute.GetCustomAttributes(MemberInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> GetAttributeRange<T>(this MemberInfo member, bool inherit = true)
        where T : Attribute
    {
        return member.GetAttributeRange( typeof( T ), inherit ).Select( DynamicCast.To<T> )!;
    }

    /// <summary>
    /// Checks if an attribute of the specified type exists for the given <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to check.</param>
    /// <param name="attributeType">Type of an <see cref="Attribute"/> to search for.</param>
    /// <param name="inherit">Specifies whether or not to include member's ancestors in the search. Equal to <b>true</b> by default.</param>
    /// <returns>
    /// <b>true</b> when an attribute of the specified type exists for the given <paramref name="member"/>, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>See <see cref="Attribute.IsDefined(MemberInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasAttribute(this MemberInfo member, Type attributeType, bool inherit = true)
    {
        return Attribute.IsDefined( member, attributeType, inherit );
    }

    /// <summary>
    /// Checks if an attribute of the specified type exists for the given <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to check.</param>
    /// <param name="inherit">Specifies whether or not to include member's ancestors in the search. Equal to <b>true</b> by default.</param>
    /// <typeparam name="T">Type of an <see cref="Attribute"/> to search for.</typeparam>
    /// <returns>
    /// <b>true</b> when an attribute of the specified type exists for the given <paramref name="member"/>, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>See <see cref="Attribute.IsDefined(MemberInfo,Type,Boolean)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasAttribute<T>(this MemberInfo member, bool inherit = true)
        where T : Attribute
    {
        return member.HasAttribute( typeof( T ), inherit );
    }

    /// <summary>
    /// Attempts to cast the given <paramref name="member"/> to <see cref="EventInfo"/> type.
    /// </summary>
    /// <param name="member">Member to cast.</param>
    /// <returns>Member cast to <see cref="EventInfo"/> or null, if it is not an instance of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EventInfo? TryAsEvent(this MemberInfo member)
    {
        return DynamicCast.TryTo<EventInfo>( member );
    }

    /// <summary>
    /// Attempts to cast the given <paramref name="member"/> to <see cref="FieldInfo"/> type.
    /// </summary>
    /// <param name="member">Member to cast.</param>
    /// <returns>Member cast to <see cref="FieldInfo"/> or null, if it is not an instance of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FieldInfo? TryAsField(this MemberInfo member)
    {
        return DynamicCast.TryTo<FieldInfo>( member );
    }

    /// <summary>
    /// Attempts to cast the given <paramref name="member"/> to <see cref="PropertyInfo"/> type.
    /// </summary>
    /// <param name="member">Member to cast.</param>
    /// <returns>Member cast to <see cref="PropertyInfo"/> or null, if it is not an instance of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PropertyInfo? TryAsProperty(this MemberInfo member)
    {
        return DynamicCast.TryTo<PropertyInfo>( member );
    }

    /// <summary>
    /// Attempts to cast the given <paramref name="member"/> to <see cref="Type"/> type.
    /// </summary>
    /// <param name="member">Member to cast.</param>
    /// <returns>Member cast to <see cref="Type"/> or null, if it is not an instance of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? TryAsType(this MemberInfo member)
    {
        return DynamicCast.TryTo<Type>( member );
    }

    /// <summary>
    /// Attempts to cast the given <paramref name="member"/> to <see cref="ConstructorInfo"/> type.
    /// </summary>
    /// <param name="member">Member to cast.</param>
    /// <returns>Member cast to <see cref="ConstructorInfo"/> or null, if it is not an instance of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ConstructorInfo? TryAsConstructor(this MemberInfo member)
    {
        return DynamicCast.TryTo<ConstructorInfo>( member );
    }

    /// <summary>
    /// Attempts to cast the given <paramref name="member"/> to <see cref="MethodInfo"/> type.
    /// </summary>
    /// <param name="member">Member to cast.</param>
    /// <returns>Member cast to <see cref="MethodInfo"/> or null, if it is not an instance of that type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MethodInfo? TryAsMethod(this MemberInfo member)
    {
        return DynamicCast.TryTo<MethodInfo>( member );
    }
}
