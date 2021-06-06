using System;

namespace LfrlSoft.NET.Common.Tests.Extensions
{
    [AttributeUsage( AttributeTargets.Class, Inherited = false )]
    public class TestClassAttribute : Attribute
    {
        public TestClassAttribute(Type? dataClass)
        {
            if ( dataClass?.IsGenericTypeDefinition != false )
                throw new ArgumentException( "Data class cannot be an open generic type." );

            DataClass = dataClass;
        }

        public Type DataClass { get; }
    }
}
