using System;
using System.Reflection;
using AutoFixture;

namespace LfrlSoft.NET.Common.Tests.Extensions
{
    [AttributeUsage( AttributeTargets.Method )]
    public class MethodDataAttribute : MethodDataAttributeBase
    {
        public MethodDataAttribute(string memberName, params object[] parameters)
            : base( memberName, new Fixture(), parameters ) { }

        public IFixture Fixture => (IFixture) Parameters[0];

        protected override Type GetTestMethodDeclaringType(Type? testClass)
        {
            if ( testClass?.IsGenericType != false )
                throw new ArgumentException( "Test class cannot be generic." );

            var memberType = testClass.GetCustomAttribute<TestClassAttribute>()?.DataClass;
            var result = memberType ?? testClass;

            return result;
        }
    }
}
