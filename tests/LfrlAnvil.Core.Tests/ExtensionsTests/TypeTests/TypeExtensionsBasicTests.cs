using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class TypeExtensionsBasicTests : TestsBase
{
    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonNestedNonGenericType()
    {
        var type = typeof( IEnumerable );
        var result = type.GetDebugString();
        result.Should().Be( "System.Collections.IEnumerable" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonNestedClosedGenericType()
    {
        var type = typeof( Dictionary<int, string> );
        var result = type.GetDebugString();
        result.Should().Be( "System.Collections.Generic.Dictionary`2[TKey is System.Int32, TValue is System.String]" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonNestedOpenGenericType()
    {
        var type = typeof( Dictionary<,> );
        var result = type.GetDebugString();
        result.Should().Be( "System.Collections.Generic.Dictionary`2[TKey, TValue]" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNestedType()
    {
        var type = typeof( Foo.Bar );
        var result = type.GetDebugString();
        result.Should().Be( "LfrlAnvil.Tests.ExtensionsTests.TypeTests.TypeExtensionsBasicTests+Foo+Bar" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForByRefType()
    {
        var type = typeof( Dictionary<int, string> )
            .GetMethod( nameof( Dictionary<int, string>.TryGetValue ) )!
            .GetParameters()[1]
            .ParameterType;

        var result = type.GetDebugString();
        result.Should().Be( "System.String&" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForOpenGenericTypeWithCovariantAndContravariantArguments()
    {
        var type = typeof( Func<,> );
        var result = type.GetDebugString();
        result.Should().Be( "System.Func`2[T [in], TResult [out]]" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForClosedGenericTypeWithCovariantAndContravariantArguments()
    {
        var type = typeof( Func<int, string> );
        var result = type.GetDebugString();
        result.Should().Be( "System.Func`2[T [in] is System.Int32, TResult [out] is System.String]" );
    }

    private sealed class Foo
    {
        internal sealed class Bar { }
    }
}
