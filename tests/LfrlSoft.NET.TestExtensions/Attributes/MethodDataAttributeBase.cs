using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.TestExtensions.Attributes
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class MethodDataAttributeBase : MemberDataAttributeBase
    {
        protected MethodDataAttributeBase(string memberName, IFixture fixture, params object[] parameters)
            : base( memberName, CreateParameters( fixture, parameters ) ) { }

        public IFixture Fixture => (IFixture)Parameters[0];

        public sealed override IEnumerable<object[]?>? GetData(MethodInfo testMethod)
        {
            if ( MemberType is not null )
                return base.GetData( testMethod );

            if ( testMethod is null )
                throw new ArgumentNullException( nameof( testMethod ) );

            var testClass = testMethod.DeclaringType;
            var testMethodDeclaringType = GetTestMethodDeclaringType( testClass );

            var func = CreateDataMethodDelegate( testMethodDeclaringType );
            if ( func is null )
                throw new ArgumentException( $"Could not find valid public static method named '{MemberName}'" );

            var obj = func();
            if ( obj is null )
                return null;

            if ( obj is IEnumerable source )
                return source.Cast<object?>().Select( o => ConvertDataItem( testMethod, o ) );

            throw new ArgumentException( $"Method '{MemberName}' did not return IEnumerable" );
        }

        protected abstract Type GetTestMethodDeclaringType(Type? testClass);

        protected sealed override object[]? ConvertDataItem(MethodInfo testMethod, object? item)
        {
            if ( item == null )
                return null;

            if ( item is object[] objArray )
                return objArray;

            var memberType = MemberType ?? testMethod.DeclaringType;
            throw new ArgumentException( $"Property {MemberName} on {memberType} yielded an item that is not an object[]" );
        }

        private Func<object?>? CreateDataMethodDelegate(Type testMethodDeclaringType)
        {
            var parameterTypes = Parameters == null ? Array.Empty<Type>() : Parameters.Select( p => p?.GetType() ).ToArray();

            var method = testMethodDeclaringType
                .GetRuntimeMethods()
                .Where(
                    m =>
                    {
                        if ( m.Name != MemberName || ! m.IsStatic )
                            return false;

                        var parameters = m.GetParameters();
                        if ( parameters.Length != parameterTypes.Length )
                            return false;

                        var zippedParams = parameters.Zip( parameterTypes, (a, b) => (First: a, Second: b) );
                        return zippedParams
                            .All( x => x.First.ParameterType.IsAssignableFrom( x.Second ) );
                    } )
                .FirstOrDefault();

            return method is null ? null : () => method.Invoke( null, Parameters );
        }

        private static object[] CreateParameters(IFixture fixture, object[]? parameters)
        {
            parameters ??= Array.Empty<object>();
            var result = new object[parameters.Length + 1];

            result[0] = fixture;
            parameters.CopyTo( result, 1 );
            return result;
        }
    }
}
