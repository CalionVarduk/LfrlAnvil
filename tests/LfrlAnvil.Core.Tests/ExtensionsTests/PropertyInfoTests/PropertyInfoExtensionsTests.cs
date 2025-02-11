using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.PropertyInfoTests;

public class PropertyInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetBackingField_ShouldReturnNull_WhenPropertyIsPublic_AndExplicit_AndWritable()
    {
        var sut = TestClass.GetPublicExplicitWritableInfo();
        var result = sut.GetBackingField();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnNull_WhenPropertyIsPublic_AndExplicit_AndWriteOnly()
    {
        var sut = TestClass.GetPublicExplicitWriteOnlyInfo();
        var result = sut.GetBackingField();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnNull_WhenPropertyIsPublic_AndExplicit_AndReadOnly()
    {
        var sut = TestClass.GetPublicExplicitReadOnlyInfo();
        var result = sut.GetBackingField();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPublic_AndAuto_AndWritable()
    {
        var sut = TestClass.GetPublicAutoWritableInfo();
        var result = sut.GetBackingField();
        Assertion.All(
                result.TestNotNull(),
                result.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            r.IsPrivate.TestTrue(),
                            r.Name.TestContains( sut.Name ),
                            Attribute.IsDefined( r, typeof( CompilerGeneratedAttribute ) ).TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPublic_AndAuto_AndReadOnly()
    {
        var sut = TestClass.GetPublicAutoReadOnlyInfo();
        var result = sut.GetBackingField();
        Assertion.All(
                result.TestNotNull(),
                result.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            r.IsPrivate.TestTrue(),
                            r.Name.TestContains( sut.Name ),
                            Attribute.IsDefined( r, typeof( CompilerGeneratedAttribute ) ).TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnNull_WhenPropertyIsPrivate_AndExplicit_AndWritable()
    {
        var sut = TestClass.GetPrivateExplicitWritableInfo();
        var result = sut.GetBackingField();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnNull_WhenPropertyIsPrivate_AndExplicit_AndWriteOnly()
    {
        var sut = TestClass.GetPrivateExplicitWriteOnlyInfo();
        var result = sut.GetBackingField();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnNull_WhenPropertyIsPrivate_AndExplicit_AndReadOnly()
    {
        var sut = TestClass.GetPrivateExplicitReadOnlyInfo();
        var result = sut.GetBackingField();
        result.TestNull().Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPrivate_AndAuto_AndWritable()
    {
        var sut = TestClass.GetPrivateAutoWritableInfo();
        var result = sut.GetBackingField();
        Assertion.All(
                result.TestNotNull(),
                result.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            r.IsPrivate.TestTrue(),
                            r.Name.TestContains( sut.Name ),
                            Attribute.IsDefined( r, typeof( CompilerGeneratedAttribute ) ).TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPrivate_AndAuto_AndReadOnly()
    {
        var sut = TestClass.GetPrivateAutoReadOnlyInfo();
        var result = sut.GetBackingField();
        Assertion.All(
                result.TestNotNull(),
                result.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            r.IsPrivate.TestTrue(),
                            r.Name.TestContains( sut.Name ),
                            Attribute.IsDefined( r, typeof( CompilerGeneratedAttribute ) ).TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void IsIndexer_ShouldReturnTrue_WhenPropertyContainsIndexParameters()
    {
        var sut = TestClass.GetIndexerInfo();
        var result = sut.IsIndexer();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsIndexer_ShouldReturnFalse_WhenPropertyDoesNotContainIndexParameters()
    {
        var sut = TestClass.GetPublicAutoWritableInfo();
        var result = sut.IsIndexer();
        result.TestFalse().Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithoutIncludedDeclaringType()
    {
        var property = typeof( string ).GetProperty( nameof( string.Length ) )!;
        var result = property.GetDebugString( includeDeclaringType: false );
        result.TestEquals( "System.Int32 Length [get]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_WithIncludedDeclaringType()
    {
        var property = typeof( string ).GetProperty( nameof( string.Length ) )!;
        var result = property.GetDebugString( includeDeclaringType: true );
        result.TestEquals( "System.Int32 System.String.Length [get]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForPropertyWithGetterAndSetter()
    {
        var property = TestClass.GetPublicAutoWritableInfo();
        var result = property.GetDebugString( includeDeclaringType: false );
        result.TestEquals( "System.Int32 PublicAutoWritableProperty [get][set]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForPropertyWithSetterOnly()
    {
        var property = TestClass.GetPublicExplicitWriteOnlyInfo();
        var result = property.GetDebugString( includeDeclaringType: false );
        result.TestEquals( "System.Int32 PublicExplicitWriteOnlyProperty [set]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForIndexerProperty()
    {
        var property = TestClass.GetIndexerInfo();
        var result = property.GetDebugString( includeDeclaringType: false );
        result.TestEquals( "System.Decimal Item[System.Int32 index] [get]" ).Go();
    }
}

public class TestClass
{
    public static PropertyInfo GetIndexerInfo()
    {
        return typeof( TestClass ).GetProperties( BindingFlags.Instance | BindingFlags.Public )
            .First( p => p.GetIndexParameters().Length > 0 );
    }

    public static PropertyInfo GetPublicExplicitWritableInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PublicExplicitWritableProperty ),
            BindingFlags.Instance | BindingFlags.Public )!;
    }

    public static PropertyInfo GetPublicExplicitWriteOnlyInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PublicExplicitWriteOnlyProperty ),
            BindingFlags.Instance | BindingFlags.Public )!;
    }

    public static PropertyInfo GetPublicExplicitReadOnlyInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PublicExplicitReadOnlyProperty ),
            BindingFlags.Instance | BindingFlags.Public )!;
    }

    public static PropertyInfo GetPublicAutoWritableInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PublicAutoWritableProperty ),
            BindingFlags.Instance | BindingFlags.Public )!;
    }

    public static PropertyInfo GetPublicAutoReadOnlyInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PublicAutoReadOnlyProperty ),
            BindingFlags.Instance | BindingFlags.Public )!;
    }

    public static PropertyInfo GetPrivateExplicitWritableInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PrivateExplicitWritableProperty ),
            BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    public static PropertyInfo GetPrivateExplicitWriteOnlyInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PrivateExplicitWriteOnlyProperty ),
            BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    public static PropertyInfo GetPrivateExplicitReadOnlyInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PrivateExplicitReadOnlyProperty ),
            BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    public static PropertyInfo GetPrivateAutoWritableInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PrivateAutoWritableProperty ),
            BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    public static PropertyInfo GetPrivateAutoReadOnlyInfo()
    {
        return typeof( TestClass ).GetProperty(
            nameof( PrivateAutoReadOnlyProperty ),
            BindingFlags.Instance | BindingFlags.NonPublic )!;
    }

    private int _explicitBackingField;

    public int PublicExplicitWritableProperty
    {
        get => _explicitBackingField;
        set => _explicitBackingField = value;
    }

    public int PublicExplicitWriteOnlyProperty
    {
        set => _explicitBackingField = value;
    }

    public int PublicExplicitReadOnlyProperty
    {
        get => _explicitBackingField;
    }

    public int PublicAutoWritableProperty { get; set; }
    public int PublicAutoReadOnlyProperty { get; }

    private int PrivateExplicitWritableProperty
    {
        get => _explicitBackingField;
        set => _explicitBackingField = value;
    }

    private int PrivateExplicitWriteOnlyProperty
    {
        set => _explicitBackingField = value;
    }

    private int PrivateExplicitReadOnlyProperty
    {
        get => _explicitBackingField;
    }

    private int PrivateAutoWritableProperty { get; set; }
    private int PrivateAutoReadOnlyProperty { get; }

    public decimal this[int index] => 0;
}
