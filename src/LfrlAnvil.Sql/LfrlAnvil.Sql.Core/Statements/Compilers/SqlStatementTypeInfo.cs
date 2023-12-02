using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements.Compilers;

internal readonly struct SqlStatementTypeInfo
{
    private SqlStatementTypeInfo(Type normalizedType, Type actualType, bool isNullable)
    {
        NormalizedType = normalizedType;
        ActualType = actualType;
        IsNullable = isNullable;
    }

    internal Type NormalizedType { get; }
    internal Type ActualType { get; }
    internal bool IsNullable { get; }

    internal bool IsReducibleCollection =>
        ActualType != typeof( byte[] ) && ActualType != typeof( string ) && ActualType.IsAssignableTo( typeof( IEnumerable ) );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static SqlStatementTypeInfo Create(ParameterInfo parameter, NullabilityInfoContext nullContext, bool forceNullable = false)
    {
        if ( parameter.ParameterType.IsValueType )
            return CreateForValueType( parameter.ParameterType, forceNullable );

        if ( forceNullable )
            return CreateForNullableRefType( parameter.ParameterType );

        var info = nullContext.Create( parameter );
        return CreateForRefType( info );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static SqlStatementTypeInfo Create(MemberInfo member, NullabilityInfoContext nullContext, bool forceNullable = false)
    {
        Assume.True( member.MemberType is MemberTypes.Field or MemberTypes.Property );

        return member.MemberType == MemberTypes.Field
            ? Create( ReinterpretCast.To<FieldInfo>( member ), nullContext, forceNullable )
            : Create( ReinterpretCast.To<PropertyInfo>( member ), nullContext, forceNullable );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static SqlStatementTypeInfo Create(FieldInfo field, NullabilityInfoContext nullContext, bool forceNullable = false)
    {
        if ( field.FieldType.IsValueType )
            return CreateForValueType( field.FieldType, forceNullable );

        if ( forceNullable )
            return CreateForNullableRefType( field.FieldType );

        var info = nullContext.Create( field );
        return CreateForRefType( info );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static SqlStatementTypeInfo Create(PropertyInfo property, NullabilityInfoContext nullContext, bool forceNullable = false)
    {
        if ( property.PropertyType.IsValueType )
            return CreateForValueType( property.PropertyType, forceNullable );

        if ( forceNullable )
            return CreateForNullableRefType( property.PropertyType );

        var info = nullContext.Create( property );
        return CreateForRefType( info );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static SqlStatementTypeInfo Create(Type type)
    {
        return type.IsValueType ? CreateForValueType( type, forceNullable: false ) : CreateForNullableRefType( type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlStatementTypeInfo CreateForValueType(Type type, bool forceNullable)
    {
        var underlyingType = Nullable.GetUnderlyingType( type );
        return underlyingType is null
            ? new SqlStatementTypeInfo( type, type, forceNullable )
            : new SqlStatementTypeInfo( underlyingType, type, true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlStatementTypeInfo CreateForRefType(NullabilityInfo info)
    {
        var state = info.WriteState == NullabilityState.Unknown ? info.ReadState : info.WriteState;
        return new SqlStatementTypeInfo( info.Type, info.Type, state != NullabilityState.NotNull );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqlStatementTypeInfo CreateForNullableRefType(Type type)
    {
        return new SqlStatementTypeInfo( type, type, true );
    }
}
