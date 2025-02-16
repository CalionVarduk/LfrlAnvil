using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectOriginalValueTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeEmpty()
    {
        var sut = default( SqlObjectOriginalValue<string> );

        Assertion.All(
                sut.Exists.TestFalse(),
                sut.Value.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void CreateEmpty_ShouldReturnEmpty()
    {
        var sut = SqlObjectOriginalValue<string>.CreateEmpty();

        Assertion.All(
                sut.Exists.TestFalse(),
                sut.Value.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnWithValue()
    {
        var value = "foo";
        var sut = SqlObjectOriginalValue<string>.Create( value );

        Assertion.All(
                sut.Exists.TestTrue(),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForEmpty()
    {
        var sut = SqlObjectOriginalValue<string>.CreateEmpty();
        var result = sut.ToString();
        result.TestEquals( "Empty<System.String>()" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForValue()
    {
        var sut = SqlObjectOriginalValue<string>.Create( "foo" );
        var result = sut.ToString();
        result.TestEquals( "Value<System.String>(foo)" ).Go();
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnParameter_ForEmpty()
    {
        var param = "bar";
        var sut = SqlObjectOriginalValue<string>.CreateEmpty();

        var result = sut.GetValueOrDefault( param );

        result.TestRefEquals( param ).Go();
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnValue_ForValue()
    {
        var value = "foo";
        var param = "bar";
        var sut = SqlObjectOriginalValue<string>.Create( value );

        var result = sut.GetValueOrDefault( param );

        result.TestRefEquals( value ).Go();
    }
}
