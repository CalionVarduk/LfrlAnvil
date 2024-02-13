using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Tests;

public class SqlPropertyChangeTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeCancelled()
    {
        var sut = default( SqlPropertyChange<string> );

        using ( new AssertionScope() )
        {
            sut.IsCancelled.Should().BeTrue();
            sut.NewValue.Should().Be( default );
            sut.State.Should().BeNull();
        }
    }

    [Fact]
    public void Cancel_ShouldCreateCancelled()
    {
        var sut = SqlPropertyChange.Cancel<string>();

        using ( new AssertionScope() )
        {
            sut.IsCancelled.Should().BeTrue();
            sut.NewValue.Should().Be( default );
            sut.State.Should().BeNull();
        }
    }

    [Fact]
    public void Create_ShouldCreateWithValue()
    {
        var value = "foo";
        var state = new object();
        var sut = SqlPropertyChange.Create( value, state );

        using ( new AssertionScope() )
        {
            sut.IsCancelled.Should().BeFalse();
            sut.NewValue.Should().BeSameAs( value );
            sut.State.Should().BeSameAs( state );
        }
    }

    [Fact]
    public void ConversionOperator_ShouldReturnWithValue()
    {
        var value = "foo";
        SqlPropertyChange<string> sut = value;

        using ( new AssertionScope() )
        {
            sut.IsCancelled.Should().BeFalse();
            sut.NewValue.Should().BeSameAs( value );
            sut.State.Should().BeNull();
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForCancelled()
    {
        var sut = SqlPropertyChange.Cancel<string>();
        var result = sut.ToString();
        result.Should().Be( "Cancel<System.String>()" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForValue()
    {
        var sut = SqlPropertyChange.Create( "foo" );
        var result = sut.ToString();
        result.Should().Be( "Set<System.String>(foo)" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForNullValue()
    {
        var sut = SqlPropertyChange.Create( (string?)null );
        var result = sut.ToString();
        result.Should().Be( "SetNull<System.String>()" );
    }
}
