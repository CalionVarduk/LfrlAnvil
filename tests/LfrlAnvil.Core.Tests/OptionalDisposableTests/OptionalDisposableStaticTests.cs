using System.IO;

namespace LfrlAnvil.Tests.OptionalDisposableTests;

public class OptionalDisposableStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = OptionalDisposable.GetUnderlyingType( null );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = OptionalDisposable.GetUnderlyingType( type );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( OptionalDisposable<IDisposable> ), typeof( IDisposable ) )]
    [InlineData( typeof( OptionalDisposable<Stream> ), typeof( Stream ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = OptionalDisposable.GetUnderlyingType( type );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( OptionalDisposable<> ).GetGenericArguments()[0];

        var result = OptionalDisposable.GetUnderlyingType( typeof( OptionalDisposable<> ) );

        result.Should().Be( expected );
    }
}
