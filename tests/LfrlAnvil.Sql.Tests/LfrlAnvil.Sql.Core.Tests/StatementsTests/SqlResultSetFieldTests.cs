using LfrlAnvil.Sql.Statements;

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

        Assertion.All(
                sut.Ordinal.TestEquals( ordinal ),
                sut.Name.TestRefEquals( name ),
                sut.IsUsed.TestEquals( isUsed ),
                sut.TypeName.TestNull(),
                sut.TypeNames.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectField_WithTypeNames()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );

        Assertion.All(
                sut.Ordinal.TestEquals( ordinal ),
                sut.Name.TestRefEquals( name ),
                sut.IsUsed.TestEquals( isUsed ),
                sut.TypeName.TestNotNull( t => t.TestEmpty() ),
                sut.TypeNames.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutUsageAndWithoutTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: false, includeTypeNames: false );
        var result = sut.ToString();
        result.TestEquals( "[3] 'foo' (unused)" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithUsageAndWithoutTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: true, includeTypeNames: false );
        var result = sut.ToString();
        result.TestEquals( "[3] 'foo'" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutUsageAndWithEmptyTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: false, includeTypeNames: true );
        var result = sut.ToString();
        result.TestEquals( "[3] 'foo' : ? (unused)" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutUsageAndWithTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: false, includeTypeNames: true );
        sut.TryAddTypeName( "int" );
        sut.TryAddTypeName( "string" );

        var result = sut.ToString();

        result.TestEquals( "[3] 'foo' : int | string (unused)" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithUsageAndWithEmptyTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: true, includeTypeNames: true );
        var result = sut.ToString();
        result.TestEquals( "[3] 'foo' : ?" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithUsageAndWithTypeNames()
    {
        var sut = new SqlResultSetField( ordinal: 3, name: "foo", isUsed: true, includeTypeNames: true );
        sut.TryAddTypeName( "int" );
        sut.TryAddTypeName( "string" );

        var result = sut.ToString();

        result.TestEquals( "[3] 'foo' : int | string" ).Go();
    }

    [Fact]
    public void TryAddTypeName_ShouldReturnFalse_WhenIncludeTypeNamesWasFalse()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: false );

        var result = sut.TryAddTypeName( "int" );

        Assertion.All(
                result.TestFalse(),
                sut.TypeName.TestNull(),
                sut.TypeNames.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                sut.TypeName.TestNotNull( t => t.TestEmpty() ),
                sut.TypeNames.TestEmpty() )
            .Go();
    }

    [Fact]
    public void TryAddTypeName_ShouldReturnTrueAndAddFirstType_WhenTypeIsValid()
    {
        var ordinal = Fixture.Create<int>();
        var name = Fixture.Create<string>();
        var isUsed = Fixture.Create<bool>();
        var sut = new SqlResultSetField( ordinal, name, isUsed, includeTypeNames: true );

        var result = sut.TryAddTypeName( "int" );

        Assertion.All(
                result.TestTrue(),
                sut.TypeName.TestEquals( "int" ),
                sut.TypeNames.TestSequence( [ "int" ] ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.TypeName.TestEquals( "int | string" ),
                sut.TypeNames.TestSequence( [ "int", "string" ] ) )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                sut.TypeName.TestEquals( "int" ),
                sut.TypeNames.TestSequence( [ "int" ] ) )
            .Go();
    }
}
