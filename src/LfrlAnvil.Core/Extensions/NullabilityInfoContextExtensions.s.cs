using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class NullabilityInfoContextExtensions
{
    [Pure]
    public static TypeNullability GetTypeNullability(this NullabilityInfoContext context, FieldInfo field)
    {
        var type = field.FieldType;
        return type.IsValueType ? TypeNullability.CreateFromValueType( type ) : CreateFromRefTypeInfo( context.Create( field ) );
    }

    [Pure]
    public static TypeNullability GetTypeNullability(this NullabilityInfoContext context, PropertyInfo property)
    {
        var type = property.PropertyType;
        return type.IsValueType ? TypeNullability.CreateFromValueType( type ) : CreateFromRefTypeInfo( context.Create( property ) );
    }

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
