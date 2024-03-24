using System.Linq;
using System.Reflection;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.NullabilityInfoContextTests;

public class NullabilityInfoContextTests : TestsBase
{
    [Fact]
    public void GetTypeNullability_ForField_ShouldReturnCorrectResult_WithNotNullValueType()
    {
        var field = typeof( Source ).GetField( nameof( Source.NotNullValueField ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( field );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeFalse();
            result.ActualType.Should().Be( typeof( int ) );
            result.UnderlyingType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForField_ShouldReturnCorrectResult_WithNullableValueType()
    {
        var field = typeof( Source ).GetField( nameof( Source.NullableValueField ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( field );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeTrue();
            result.ActualType.Should().Be( typeof( int? ) );
            result.UnderlyingType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForField_ShouldReturnCorrectResult_WithNotNullRefType()
    {
        var field = typeof( Source ).GetField( nameof( Source.NotNullRefField ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( field );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeFalse();
            result.ActualType.Should().Be( typeof( string ) );
            result.UnderlyingType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForField_ShouldReturnCorrectResult_WithNullableRefType()
    {
        var field = typeof( Source ).GetField( nameof( Source.NullableRefField ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( field );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeTrue();
            result.ActualType.Should().Be( typeof( string ) );
            result.UnderlyingType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForProperty_ShouldReturnCorrectResult_WithNotNullValueType()
    {
        var property = typeof( Source ).GetProperty( nameof( Source.NotNullValueProperty ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( property );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeFalse();
            result.ActualType.Should().Be( typeof( int ) );
            result.UnderlyingType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForProperty_ShouldReturnCorrectResult_WithNullableValueType()
    {
        var property = typeof( Source ).GetProperty( nameof( Source.NullableValueProperty ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( property );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeTrue();
            result.ActualType.Should().Be( typeof( int? ) );
            result.UnderlyingType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForProperty_ShouldReturnCorrectResult_WithNotNullRefType()
    {
        var property = typeof( Source ).GetProperty( nameof( Source.NotNullRefProperty ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( property );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeFalse();
            result.ActualType.Should().Be( typeof( string ) );
            result.UnderlyingType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForProperty_ShouldReturnCorrectResult_WithNullableRefType()
    {
        var property = typeof( Source ).GetProperty( nameof( Source.NullableRefProperty ) )!;
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( property );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeTrue();
            result.ActualType.Should().Be( typeof( string ) );
            result.UnderlyingType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForParameter_ShouldReturnCorrectResult_WithNotNullValueType()
    {
        var parameter = typeof( Source ).GetConstructors().First().GetParameters().First( p => p.Name == "notNullValueParameter" );
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( parameter );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeFalse();
            result.ActualType.Should().Be( typeof( int ) );
            result.UnderlyingType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForParameter_ShouldReturnCorrectResult_WithNullableValueType()
    {
        var parameter = typeof( Source ).GetConstructors().First().GetParameters().First( p => p.Name == "nullableValueParameter" );
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( parameter );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeTrue();
            result.ActualType.Should().Be( typeof( int? ) );
            result.UnderlyingType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForParameter_ShouldReturnCorrectResult_WithNotNullRefType()
    {
        var parameter = typeof( Source ).GetConstructors().First().GetParameters().First( p => p.Name == "notNullRefParameter" );
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( parameter );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeFalse();
            result.ActualType.Should().Be( typeof( string ) );
            result.UnderlyingType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetTypeNullability_ForParameter_ShouldReturnCorrectResult_WithNullableRefType()
    {
        var parameter = typeof( Source ).GetConstructors().First().GetParameters().First( p => p.Name == "nullableRefParameter" );
        var sut = new NullabilityInfoContext();

        var result = sut.GetTypeNullability( parameter );

        using ( new AssertionScope() )
        {
            result.IsNullable.Should().BeTrue();
            result.ActualType.Should().Be( typeof( string ) );
            result.UnderlyingType.Should().Be( typeof( string ) );
        }
    }

    private sealed class Source
    {
        public readonly int NotNullValueField = 0;
        public readonly int? NullableValueField = null;
        public readonly string NotNullRefField = string.Empty;
        public readonly string? NullableRefField = null;
        public int NotNullValueProperty { get; }
        public int? NullableValueProperty { get; }
        public string NotNullRefProperty { get; } = string.Empty;
        public string? NullableRefProperty { get; }

        public Source(int notNullValueParameter, int? nullableValueParameter, string notNullRefParameter, string? nullableRefParameter) { }
    }
}
