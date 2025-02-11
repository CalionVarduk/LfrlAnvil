namespace LfrlAnvil.Functional.Tests.MutationTests;

public class MutationStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Mutation.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Mutation.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Mutation<int> ), typeof( int ) )]
    [InlineData( typeof( Mutation<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Mutation<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Mutation.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Mutation<> ).GetGenericArguments()[0];

        var result = Mutation.GetUnderlyingType( typeof( Mutation<> ) );

        result.TestEquals( expected ).Go();
    }
}
