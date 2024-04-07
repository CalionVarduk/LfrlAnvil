using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Statements.Compilers;

internal static class TypeHelpers
{
    internal const BindingFlags PublicMember = BindingFlags.Public | BindingFlags.Instance;
    internal const BindingFlags PublicDeclaredMember = PublicMember | BindingFlags.DeclaredOnly;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsReducibleCollection(Type type)
    {
        return type != typeof( byte[] ) && type != typeof( string ) && type.IsAssignableTo( typeof( IEnumerable ) );
    }

    [Pure]
    internal static ConstructorInfo GetResultSetFieldCtor(Type type)
    {
        var result = type.GetConstructor( new[] { typeof( int ), typeof( string ), typeof( bool ), typeof( bool ) } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo GetResultSetFieldTryAddTypeNameMethod(Type type)
    {
        var result = type.GetMethod( nameof( SqlResultSetField.TryAddTypeName ), PublicMember, new[] { typeof( string ) } );
        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static ConstructorInfo GetQueryReaderResultCtor(Type type, Type resultSetFieldArrayType, Type rowListType)
    {
        var result = type.GetConstructor( new[] { resultSetFieldArrayType, rowListType } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetQueryReaderOptionsInitialBufferCapacityProperty()
    {
        var result = typeof( SqlQueryReaderOptions ).GetProperty( nameof( SqlQueryReaderOptions.InitialBufferCapacity ) );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static ConstructorInfo GetAsyncReaderInitResultCtor()
    {
        var result = typeof( SqlAsyncQueryReaderInitResult ).GetConstructor(
            PublicMember,
            new[] { typeof( int[] ), typeof( SqlResultSetField[] ) } );

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetNullableHasValueProperty(Type type)
    {
        var result = type.GetProperty( nameof( Nullable<int>.HasValue ) );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetNullableValueProperty(Type type)
    {
        var result = type.GetProperty( nameof( Nullable<int>.Value ) );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo GetIntToStringMethod()
    {
        var result = typeof( int ).GetMethod( nameof( ToString ), PublicMember, Type.EmptyTypes );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo GetStringConcatMethod()
    {
        var result = typeof( string ).GetMethod(
            nameof( string.Concat ),
            BindingFlags.Public | BindingFlags.Static,
            new[] { typeof( string ), typeof( string ) } );

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo GetStringHashSetContainsMethod()
    {
        var result = typeof( HashSet<string> ).GetMethod( nameof( HashSet<string>.Contains ), PublicMember, new[] { typeof( string ) } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetArrayLengthProperty(Type type)
    {
        var result = type.GetProperty( nameof( Array.Length ), typeof( int ), Type.EmptyTypes );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static ConstructorInfo GetArrayCtor(Type type)
    {
        var result = type.GetConstructor( new[] { typeof( int ) } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static ConstructorInfo GetListDefaultCtor(Type type)
    {
        var result = type.GetConstructor( Type.EmptyTypes );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static ConstructorInfo GetListCtorWithCapacity(Type type)
    {
        var result = type.GetConstructor( new[] { typeof( int ) } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo GetListAddMethod(Type type, Type elementType)
    {
        var result = type.GetMethod( nameof( List<int>.Add ), PublicMember, new[] { elementType } );
        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static ConstructorInfo GetScalarQueryResultCtor(Type type)
    {
        var scalarResultType = typeof( SqlScalarQueryResult<> ).MakeGenericType( type );
        var result = scalarResultType.GetConstructor( PublicMember, new[] { type } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static FieldInfo GetScalarQueryResultEmptyField(Type type)
    {
        var result = type.GetField( nameof( SqlScalarQueryResult.Empty ), BindingFlags.Public | BindingFlags.Static );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo GetColumnTypeDefinitionToParameterValueMethod(Type type, Type valueType)
    {
        var result = type.FindMember(
            t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod
                        || m.ReturnType != typeof( object )
                        || m.Name != nameof( ISqlColumnTypeDefinition<int>.ToParameterValue ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && valueType.IsAssignableTo( parameters[0].ParameterType ) )
                        return m;
                }

                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod
                        || m.ReturnType != typeof( object )
                        || m.Name != nameof( ISqlColumnTypeDefinition.TryToParameterValue ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof( object ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetColumnTypeDefinitionSetParameterInfoMethod(Type type, Type parameterType)
    {
        var result = type.FindMember(
            t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod
                        || m.ReturnType != typeof( void )
                        || m.Name != nameof( ISqlColumnTypeDefinition.SetParameterInfo ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length != 2 || parameters[1].ParameterType != typeof( bool ) )
                        continue;

                    var p1 = parameters[0].ParameterType;
                    if ( p1 != typeof( IDbDataParameter ) && parameterType.IsAssignableTo( p1 ) )
                        return m;
                }

                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod
                        || m.ReturnType != typeof( void )
                        || m.Name != nameof( ISqlColumnTypeDefinition.SetParameterInfo ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 2
                        && parameters[0].ParameterType == typeof( IDbDataParameter )
                        && parameters[1].ParameterType == typeof( bool ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataRecordGetNameMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod || m.ReturnType != typeof( string ) || m.Name != nameof( IDataRecord.GetName ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof( int ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDataRecordFieldCountProperty(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.PropertyType == typeof( int )
                        && p.GetGetMethod() is not null
                        && p.Name == nameof( IDataRecord.FieldCount )
                        && p.GetIndexParameters().Length == 0 )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataRecordGetDataTypeNameMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod || m.ReturnType != typeof( string ) || m.Name != nameof( IDataRecord.GetDataTypeName ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof( int ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataRecordGetOrdinalMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod || m.ReturnType != typeof( int ) || m.Name != nameof( IDataRecord.GetOrdinal ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof( string ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataRecordIsDbNullMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod || m.ReturnType != typeof( bool ) || m.Name != nameof( IDataRecord.IsDBNull ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof( int ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataReaderReadMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( ! m.IsGenericMethod
                        && m.ReturnType == typeof( bool )
                        && m.Name == nameof( IDataReader.Read )
                        && m.GetParameters().Length == 0 )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDbCommandParametersProperty(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.GetGetMethod() is not null
                        && p.Name == nameof( IDbCommand.Parameters )
                        && p.PropertyType.IsAssignableTo( typeof( IDataParameterCollection ) )
                        && p.GetIndexParameters().Length == 0 )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDbCommandCreateParameterMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( ! m.IsGenericMethod
                        && m.Name == nameof( IDbCommand.CreateParameter )
                        && m.ReturnType.IsAssignableTo( typeof( IDbDataParameter ) )
                        && m.GetParameters().Length == 0 )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDataParameterCollectionCountProperty(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.PropertyType == typeof( int )
                        && p.GetGetMethod() is not null
                        && p.Name == nameof( IDataParameterCollection.Count )
                        && p.GetIndexParameters().Length == 0 )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDataParameterCollectionIndexer(Type type, Type parameterType)
    {
        var result = type.FindMember(
            t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.GetGetMethod() is null
                        || (t.IsInterface ? p.PropertyType != typeof( object ) : ! p.PropertyType.IsAssignableTo( parameterType )) )
                        continue;

                    var indexParameters = p.GetIndexParameters();
                    if ( indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof( int ) )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataParameterCollectionAddMethod(Type type, Type parameterType)
    {
        var result = type.FindMember(
            t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod || m.Name != nameof( IDataParameterCollection.Add ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length != 1 )
                        continue;

                    var p = parameters[0].ParameterType;
                    if ( t.IsInterface ? p == typeof( object ) : p != typeof( object ) && parameterType.IsAssignableTo( p ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataParameterCollectionClearMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( ! m.IsGenericMethod && m.Name == nameof( IDataParameterCollection.Clear ) && m.GetParameters().Length == 0 )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static MethodInfo GetDataParameterCollectionRemoveAtMethod(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var methods = t.GetMethods( PublicDeclaredMember );
                foreach ( var m in methods )
                {
                    if ( m.IsGenericMethod || m.Name != nameof( IDataParameterCollection.RemoveAt ) )
                        continue;

                    var parameters = m.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof( int ) )
                        return m;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDataParameterDirectionProperty(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.PropertyType == typeof( ParameterDirection )
                        && p.GetSetMethod() is not null
                        && p.Name == nameof( IDataParameter.Direction )
                        && p.GetIndexParameters().Length == 0 )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDataParameterNameProperty(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.PropertyType == typeof( string )
                        && p.GetSetMethod() is not null
                        && p.Name == nameof( IDataParameter.ParameterName )
                        && p.GetIndexParameters().Length == 0 )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }

    [Pure]
    internal static PropertyInfo GetDataParameterValueProperty(Type type)
    {
        var result = type.FindMember(
            static t =>
            {
                var properties = t.GetProperties( PublicDeclaredMember );
                foreach ( var p in properties )
                {
                    if ( p.PropertyType == typeof( object )
                        && p.GetSetMethod() is not null
                        && p.Name == nameof( IDataParameter.Value )
                        && p.GetIndexParameters().Length == 0 )
                        return p;
                }

                return null;
            } );

        Assume.IsNotNull( result?.DeclaringType );
        return result;
    }
}
