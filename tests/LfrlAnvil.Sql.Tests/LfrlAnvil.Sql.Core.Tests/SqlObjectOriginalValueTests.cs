using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectOriginalValueTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeEmpty()
    {
        var sut = default( SqlObjectOriginalValue<string> );

        using ( new AssertionScope() )
        {
            sut.Exists.Should().BeFalse();
            sut.Value.Should().Be( default );
        }
    }

    [Fact]
    public void CreateEmpty_ShouldReturnEmpty()
    {
        var sut = SqlObjectOriginalValue<string>.CreateEmpty();

        using ( new AssertionScope() )
        {
            sut.Exists.Should().BeFalse();
            sut.Value.Should().Be( default );
        }
    }

    [Fact]
    public void Create_ShouldReturnWithValue()
    {
        var value = "foo";
        var sut = SqlObjectOriginalValue<string>.Create( value );

        using ( new AssertionScope() )
        {
            sut.Exists.Should().BeTrue();
            sut.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForEmpty()
    {
        var sut = SqlObjectOriginalValue<string>.CreateEmpty();
        var result = sut.ToString();
        result.Should().Be( "Empty<System.String>()" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForValue()
    {
        var sut = SqlObjectOriginalValue<string>.Create( "foo" );
        var result = sut.ToString();
        result.Should().Be( "Value<System.String>(foo)" );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnParameter_ForEmpty()
    {
        var param = "bar";
        var sut = SqlObjectOriginalValue<string>.CreateEmpty();

        var result = sut.GetValueOrDefault( param );

        result.Should().BeSameAs( param );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnValue_ForValue()
    {
        var value = "foo";
        var param = "bar";
        var sut = SqlObjectOriginalValue<string>.Create( value );

        var result = sut.GetValueOrDefault( param );

        result.Should().BeSameAs( value );
    }
}
