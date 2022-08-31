using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.ConstructorInfoTests;

public class ConstructorInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForCtorWithoutIncludingDeclaringType()
    {
        var ctor = typeof( TestCtorClass ).GetConstructor( new[] { typeof( int ), typeof( string ) } )!;
        var result = ctor.GetDebugString( includeDeclaringType: false );
        result.Should().Be( ".ctor(System.Int32 a, System.String b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForCtorWithIncludingDeclaringType()
    {
        var ctor = typeof( TestCtorClass ).GetConstructor( new[] { typeof( int ), typeof( string ) } )!;
        var result = ctor.GetDebugString( includeDeclaringType: true );
        result.Should().Be( "LfrlAnvil.Tests.ExtensionsTests.ConstructorInfoTests.TestCtorClass..ctor(System.Int32 a, System.String b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForParameterlessCtor()
    {
        var ctor = typeof( TestCtorClass ).GetConstructor( Type.EmptyTypes )!;
        var result = ctor.GetDebugString( includeDeclaringType: false );
        result.Should().Be( ".ctor()" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForCtorWithByRefParameters()
    {
        var ctor = typeof( TestCtorClass ).GetConstructors().First( c => c.GetParameters().Length == 3 );
        var result = ctor.GetDebugString( includeDeclaringType: false );
        result.Should().Be( ".ctor(System.String& a [in], System.Decimal& b, System.Int32& c [out])" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForCtorFromOpenedGenericType()
    {
        var ctor = typeof( TestCtorGenericClass<,> ).GetConstructors().First();
        var result = ctor.GetDebugString( includeDeclaringType: false );
        result.Should().Be( ".ctor(T1 a, T2 b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForCtorFromClosedGenericType()
    {
        var ctor = typeof( TestCtorGenericClass<int, string> ).GetConstructors().First();
        var result = ctor.GetDebugString( includeDeclaringType: false );
        result.Should().Be( ".ctor(System.Int32 a, System.String b)" );
    }
}

public sealed class TestCtorClass
{
    public TestCtorClass() { }
    public TestCtorClass(int a, string b) { }

    public TestCtorClass(in string a, ref decimal b, out int c)
    {
        c = 0;
    }
}

public sealed class TestCtorGenericClass<T1, T2>
{
    public TestCtorGenericClass(T1 a, T2 b) { }
}
