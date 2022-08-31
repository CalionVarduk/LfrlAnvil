using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.FieldInfoTests;

public class FieldInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithoutIncludedDeclaringType()
    {
        var field = typeof( TestFieldClass ).GetField( nameof( TestFieldClass.TestField ) )!;
        var result = field.GetDebugString( includeDeclaringType: false );
        result.Should().Be( "System.Int32 TestField" );
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithIncludedDeclaringType()
    {
        var field = typeof( TestFieldClass ).GetField( nameof( TestFieldClass.TestField ) )!;
        var result = field.GetDebugString( includeDeclaringType: true );
        result.Should().Be( "System.Int32 LfrlAnvil.Tests.ExtensionsTests.FieldInfoTests.TestFieldClass.TestField" );
    }
}

public sealed class TestFieldClass
{
    public int TestField;
}
