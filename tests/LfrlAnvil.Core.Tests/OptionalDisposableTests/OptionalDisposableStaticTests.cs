using System.IO;

namespace LfrlAnvil.Tests.OptionalDisposableTests;

public class OptionalDisposableStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = OptionalDisposable.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = OptionalDisposable.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( OptionalDisposable<IDisposable> ), typeof( IDisposable ) )]
    [InlineData( typeof( OptionalDisposable<Stream> ), typeof( Stream ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = OptionalDisposable.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( OptionalDisposable<> ).GetGenericArguments()[0];

        var result = OptionalDisposable.GetUnderlyingType( typeof( OptionalDisposable<> ) );

        result.TestEquals( expected ).Go();
    }
}
