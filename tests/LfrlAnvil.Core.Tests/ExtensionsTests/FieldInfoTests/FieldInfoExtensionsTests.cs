using System.Reflection;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.FieldInfoTests;

public class FieldInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetBackedProperty_ShouldReturnNull_WhenFieldIsPublic()
    {
        var sut = TestFieldClass.GetPublicFieldInfo();
        var result = sut.GetBackedProperty();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackedProperty_ShouldReturnNull_WhenFieldIsPrivate_AndExplicit()
    {
        var sut = TestFieldClass.GetPrivateFieldInfo();
        var result = sut.GetBackedProperty();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackedProperty_ShouldReturnPropertyInfo_WhenFieldIsBackingFieldForPublicProperty()
    {
        var expected = TestFieldClass.GetPublicPropertyInfo();
        var sut = expected.GetBackingField()!;

        var result = sut.GetBackedProperty();

        result.TestRefEquals( expected ).Go();
    }

    [Fact]
    public void GetBackedProperty_ShouldReturnPropertyInfo_WhenFieldIsBackingFieldForPrivateProperty()
    {
        var expected = TestFieldClass.GetPrivatePropertyInfo();
        var sut = expected.GetBackingField()!;

        var result = sut.GetBackedProperty();

        result.TestRefEquals( expected ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithoutIncludedDeclaringType()
    {
        var field = TestFieldClass.GetPublicFieldInfo();
        var result = field.GetDebugString( includeDeclaringType: false );
        result.TestEquals( "System.Int32 TestField" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithIncludedDeclaringType()
    {
        var field = TestFieldClass.GetPublicFieldInfo();
        var result = field.GetDebugString( includeDeclaringType: true );
        result.TestEquals( "System.Int32 LfrlAnvil.Tests.ExtensionsTests.FieldInfoTests.TestFieldClass.TestField" ).Go();
    }
}

public sealed class TestFieldClass
{
    public static FieldInfo GetPublicFieldInfo()
    {
        return typeof( TestFieldClass ).GetField( nameof( TestField ) )!;
    }

    public static FieldInfo GetPrivateFieldInfo()
    {
        return typeof( TestFieldClass ).GetField( nameof( TestPrivateField ), BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    public static PropertyInfo GetPublicPropertyInfo()
    {
        return typeof( TestFieldClass ).GetProperty( nameof( TestProperty ) )!;
    }

    public static PropertyInfo GetPrivatePropertyInfo()
    {
        return typeof( TestFieldClass ).GetProperty( nameof( TestPrivateProperty ), BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    public int TestField;
    private string? TestPrivateField = null;

    public int TestProperty { get; set; }
    private string? TestPrivateProperty { get; set; }
}
