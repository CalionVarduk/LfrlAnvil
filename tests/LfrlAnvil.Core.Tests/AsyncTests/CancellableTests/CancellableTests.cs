using System.Threading;
using FluentAssertions.Execution;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests.CancellableTests;

public class CancellableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<string>();
        var token = new CancellationToken( false );

        var sut = new Cancellable<string>( value, token );

        using ( new AssertionScope() )
        {
            sut.Value.Should().Be( value );
            sut.Token.Should().Be( token );
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<string>();
        var token = new CancellationToken( false );

        var sut = Cancellable.Create( value, token );

        using ( new AssertionScope() )
        {
            sut.Value.Should().Be( value );
            sut.Token.Should().Be( token );
        }
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

        result.Should().Be( $"Value: {value}, IsCancellationRequested: {expectedCancelledText}" );
    }
}
