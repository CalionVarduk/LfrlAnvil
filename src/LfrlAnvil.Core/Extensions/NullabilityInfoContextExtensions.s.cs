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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="NullabilityInfoContext"/> extension methods.
/// </summary>
public static class NullabilityInfoContextExtensions
{
    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance for the specified <paramref name="field"/>
    /// using the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">Source context.</param>
    /// <param name="field"><see cref="FieldInfo"/> to create <see cref="TypeNullability"/> for.</param>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public static TypeNullability GetTypeNullability(this NullabilityInfoContext context, FieldInfo field)
    {
        var type = field.FieldType;
        return type.IsValueType ? TypeNullability.CreateFromValueType( type ) : CreateFromRefTypeInfo( context.Create( field ) );
    }

    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance for the specified <paramref name="property"/>
    /// using the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">Source context.</param>
    /// <param name="property"><see cref="PropertyInfo"/> to create <see cref="TypeNullability"/> for.</param>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public static TypeNullability GetTypeNullability(this NullabilityInfoContext context, PropertyInfo property)
    {
        var type = property.PropertyType;
        return type.IsValueType ? TypeNullability.CreateFromValueType( type ) : CreateFromRefTypeInfo( context.Create( property ) );
    }

    /// <summary>
    /// Creates a new <see cref="TypeNullability"/> instance for the specified <paramref name="parameter"/>
    /// using the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">Source context.</param>
    /// <param name="parameter"><see cref="ParameterInfo"/> to create <see cref="TypeNullability"/> for.</param>
    /// <returns>New <see cref="TypeNullability"/> instance.</returns>
    [Pure]
    public static TypeNullability GetTypeNullability(this NullabilityInfoContext context, ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        return type.IsValueType ? TypeNullability.CreateFromValueType( type ) : CreateFromRefTypeInfo( context.Create( parameter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static TypeNullability CreateFromRefTypeInfo(NullabilityInfo info)
    {
        var state = info.WriteState != NullabilityState.Unknown ? info.WriteState : info.ReadState;
        return TypeNullability.CreateFromRefType( info.Type, isNullable: state != NullabilityState.NotNull );
    }
}
