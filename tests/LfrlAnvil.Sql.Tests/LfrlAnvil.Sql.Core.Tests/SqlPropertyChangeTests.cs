using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Tests;

public class SqlPropertyChangeTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeCancelled()
    {
        var sut = default( SqlPropertyChange<string> );

        Assertion.All(
                sut.IsCancelled.TestTrue(),
                sut.NewValue.TestEquals( default ),
                sut.State.TestNull() )
            .Go();
    }

    [Fact]
    public void Cancel_ShouldCreateCancelled()
    {
        var sut = SqlPropertyChange.Cancel<string>();

        Assertion.All(
                sut.IsCancelled.TestTrue(),
                sut.NewValue.TestEquals( default ),
                sut.State.TestNull() )
            .Go();
    }

    [Fact]
    public void Create_ShouldCreateWithValue()
    {
        var value = "foo";
        var state = new object();
        var sut = SqlPropertyChange.Create( value, state );

        Assertion.All(
                sut.IsCancelled.TestFalse(),
                sut.NewValue.TestRefEquals( value ),
                sut.State.TestRefEquals( state ) )
            .Go();
    }

    [Fact]
    public void ConversionOperator_ShouldReturnWithValue()
    {
        var value = "foo";
        SqlPropertyChange<string> sut = value;

        Assertion.All(
                sut.IsCancelled.TestFalse(),
                sut.NewValue.TestRefEquals( value ),
                sut.State.TestNull() )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForCancelled()
    {
        var sut = SqlPropertyChange.Cancel<string>();
        var result = sut.ToString();
        result.TestEquals( "Cancel<System.String>()" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForValue()
    {
        var sut = SqlPropertyChange.Create( "foo" );
        var result = sut.ToString();
        result.TestEquals( "Set<System.String>(foo)" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_ForNullValue()
    {
        var sut = SqlPropertyChange.Create( ( string? )null );
        var result = sut.ToString();
        result.TestEquals( "SetNull<System.String>()" ).Go();
    }
}
