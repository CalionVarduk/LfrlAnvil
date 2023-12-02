using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlResultSetFieldTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateCorrectField_WithoutTypeNames()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: false );

        using ( new AssertionScope() )
        {
            sut.Ordinal.Should().Be( ordinal );
            sut.Name.Should().BeSameAs( name );
            sut.IsUsed.Should().Be( isUsed );
            sut.TypeName.Should().BeNull();
            sut.TypeNames.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectField_WithTypeNames()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );

        using ( new AssertionScope() )
        {
            sut.Ordinal.Should().Be( ordinal );
            sut.Name.Should().BeSameAs( name );
            sut.IsUsed.Should().Be( isUsed );
            sut.TypeName.Should().BeEmpty();
            sut.TypeNames.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutUsageAndWithoutTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: false, includeTypeNames: false );
        var result = sut.ToString();
        result.Should().Be( "[3] 'foo' (unused)" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithUsageAndWithoutTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: true, includeTypeNames: false );
        var result = sut.ToString();
        result.Should().Be( "[3] 'foo'" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutUsageAndWithEmptyTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: false, includeTypeNames: true );
        var result = sut.ToString();
        result.Should().Be( "[3] 'foo' : ? (unused)" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutUsageAndWithTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: false, includeTypeNames: true );
        sut.TryAddTypeName( "int" );
        sut.TryAddTypeName( "string" );

        var result = sut.ToString();

        result.Should().Be( "[3] 'foo' : int | string (unused)" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithUsageAndWithEmptyTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: true, includeTypeNames: true );
        var result = sut.ToString();
        result.Should().Be( "[3] 'foo' : ?" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithUsageAndWithTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: true, includeTypeNames: true );
        sut.TryAddTypeName( "int" );
        sut.TryAddTypeName( "string" );

        var result = sut.ToString();

        result.Should().Be( "[3] 'foo' : int | string" );
    }

    [Fact]
    public void TryAddTypeName_ShouldReturnFalse_WhenIncludeTypeNamesWasFalse()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: false );

        var result = sut.TryAddTypeName( "int" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TypeName.Should().BeNull();
            sut.TypeNames.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "" )]
    public void TryAddTypeName_ShouldReturnFalse_WhenNameIsNullOrEmpty(string? typeName)
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );

        var result = sut.TryAddTypeName( typeName );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TypeName.Should().BeEmpty();
            sut.TypeNames.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void TryAddTypeName_ShouldReturnTrueAndAddFirstType_WhenTypeIsValid()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );

        var result = sut.TryAddTypeName( "int" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TypeName.Should().Be( "int" );
            sut.TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int" );
        }
    }

    [Fact]
    public void TryAddTypeName_ShouldReturnTrueAndAddNextType_WhenTypeIsValidAndDoesNotExist()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );
        sut.TryAddTypeName( "int" );

        var result = sut.TryAddTypeName( "string" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TypeName.Should().Be( "int | string" );
            sut.TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int", "string" );
        }
    }

    [Fact]
    public void TryAddTypeName_ShouldReturnFalse_WhenTypeIsValidButExists()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );
        sut.TryAddTypeName( "int" );

        var result = sut.TryAddTypeName( "int" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TypeName.Should().Be( "int" );
            sut.TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int" );
        }
    }
}
