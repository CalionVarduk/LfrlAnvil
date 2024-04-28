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
