using System.Reflection;

namespace LfrlAnvil.TestExtensions.Attributes;

[AttributeUsage( AttributeTargets.Method )]
public class GenericMethodDataAttribute : MethodDataAttributeBase
{
    public GenericMethodDataAttribute(string memberName, params object[] parameters)
        : base( memberName, new Fixture(), parameters ) { }

    protected override Type GetTestMethodDeclaringType(Type? testClass)
    {
        if ( testClass?.IsNested == true )
            throw new ArgumentException( "Nested test classes aren't supported." );

        if ( testClass?.IsGenericType != true )
            throw new ArgumentException( "Test class must be generic." );

        var openMemberType = testClass.GetCustomAttribute<GenericTestClassAttribute>()?.DataClass;
        var result = openMemberType is null ? testClass : openMemberType.MakeGenericType( testClass.GetGenericArguments() );

        return result;
    }
}