namespace LfrlAnvil.Tests.ReadOnlyArrayTests;

public class ReadOnlyArrayStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = ReadOnlyArray.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = ReadOnlyArray.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( ReadOnlyArray<int> ), typeof( int ) )]
    [InlineData( typeof( ReadOnlyArray<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( ReadOnlyArray<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = ReadOnlyArray.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( ReadOnlyArray<> ).GetGenericArguments()[0];

        var result = ReadOnlyArray.GetUnderlyingType( typeof( ReadOnlyArray<> ) );

        result.TestEquals( expected ).Go();
    }
}
