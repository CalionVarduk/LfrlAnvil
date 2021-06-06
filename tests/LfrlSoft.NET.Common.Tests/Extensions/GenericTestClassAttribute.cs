using System;

namespace LfrlSoft.NET.Common.Tests.Extensions
{
    [AttributeUsage( AttributeTargets.Class, Inherited = false )]
    public class GenericTestClassAttribute : Attribute
    {
        public GenericTestClassAttribute(Type? dataClass)
        {
            if ( dataClass?.IsGenericTypeDefinition != true )
                throw new ArgumentException( "Data class must be an open generic type." );

            DataClass = dataClass;
        }

        public Type DataClass { get; }
    }
}
