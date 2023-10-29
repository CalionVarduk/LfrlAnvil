using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.MethodInfoTests;

public class MethodInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonGenericMethodWithoutIncludingDeclaringType()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.NonGeneric ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "System.Int32 NonGeneric(System.String a, System.Decimal b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonGenericMethodWithIncludingDeclaringType()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.NonGeneric ) )!;
        var result = method.GetDebugString( includeDeclaringType: true );

        result.Should()
            .Be(
                "System.Int32 LfrlAnvil.Tests.ExtensionsTests.MethodInfoTests.TestMethodClass.NonGeneric(System.String a, System.Decimal b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForParameterlessMethod()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.Parameterless ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "System.String Parameterless()" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForMethodWithByRefParameters()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.ByRef ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "System.Int32& ByRef(System.String& a [in], System.Decimal& b, System.Int32& c [out])" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForOpenedGenericMethod()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.Generic ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "T1 Generic`3[T1, T2, T3](T3 a, T2 b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForGenericMethodWithIncludingDeclaringType()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.Generic ) )!;
        var result = method.GetDebugString( includeDeclaringType: true );
        result.Should().Be( "T1 LfrlAnvil.Tests.ExtensionsTests.MethodInfoTests.TestMethodClass.Generic`3[T1, T2, T3](T3 a, T2 b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForClosedGenericMethod()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.Generic ) )!.MakeGenericMethod(
            typeof( int ),
            typeof( string ),
            typeof( decimal ) );

        var result = method.GetDebugString( includeDeclaringType: false );

        result.Should().Be( "T1 Generic`3[T1 is System.Int32, T2 is System.String, T3 is System.Decimal](T3 a, T2 b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForOpenedPartiallyGenericMethod()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.PartiallyGeneric ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "System.Int32 PartiallyGeneric`2[T1, T2](T2 a, System.String b, T1 c, System.Int32 d)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForClosedPartiallyGenericMethod()
    {
        var method = typeof( TestMethodClass ).GetMethod( nameof( TestMethodClass.PartiallyGeneric ) )!.MakeGenericMethod(
            typeof( float ),
            typeof( decimal ) );

        var result = method.GetDebugString( includeDeclaringType: false );

        result.Should()
            .Be(
                "System.Int32 PartiallyGeneric`2[T1 is System.Single, T2 is System.Decimal](T2 a, System.String b, T1 c, System.Int32 d)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForGenericMethodFromOpenedGenericType()
    {
        var method = typeof( TestMethodGenericClass<,> ).GetMethod( nameof( TestMethodGenericClass<int, int>.Method ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "T1 Method`1[T3](T2 a, T3 b)" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForGenericMethodFromClosedGenericType()
    {
        var method = typeof( TestMethodGenericClass<int, string> ).GetMethod( nameof( TestMethodGenericClass<int, int>.Method ) )!;
        var result = method.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "System.Int32 Method`1[T3](System.String a, T3 b)" );
    }
}

public sealed class TestMethodClass
{
    private static int _x;

    public static string Parameterless()
    {
        return string.Empty;
    }

    public static int NonGeneric(string a, decimal b)
    {
        return 0;
    }

    public static ref int ByRef(in string a, ref decimal b, out int c)
    {
        c = 0;
        return ref _x;
    }

    public static T1? Generic<T1, T2, T3>(T3 a, T2 b)
    {
        return default;
    }

    public static int PartiallyGeneric<T1, T2>(T2 a, string b, T1 c, int d)
    {
        return 0;
    }
}

public sealed class TestMethodGenericClass<T1, T2>
{
    public static T1? Method<T3>(T2 a, T3 b)
    {
        return default;
    }
}
