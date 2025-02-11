using System.Threading;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class CancellableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<string>();
        var token = new CancellationToken( false );

        var sut = new Cancellable<string>( value, token );

        Assertion.All( sut.Value.TestEquals( value ), sut.Token.TestEquals( token ) ).Go();
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<string>();
        var token = new CancellationToken( false );

        var sut = Cancellable.Create( value, token );

        Assertion.All( sut.Value.TestEquals( value ), sut.Token.TestEquals( token ) ).Go();
    }

    [Theory]
    [InlineData( true, "True" )]
    [InlineData( false, "False" )]
    public void ToString_ShouldReturnCorrectResult(bool cancelled, string expectedCancelledText)
    {
        var value = Fixture.Create<string>();
        var token = new CancellationToken( cancelled );
        var sut = new Cancellable<string>( value, token );

        var result = sut.ToString();

        result.TestEquals( $"Value: {value}, IsCancellationRequested: {expectedCancelledText}" ).Go();
    }
}
