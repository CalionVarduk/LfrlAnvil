namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerExternalObjectTests : TestsBase
{
    [Theory]
    [InlineData( 0, null )]
    [InlineData( 1, "foo" )]
    [InlineData( 2, "bar" )]
    public void Ctor_ShouldCreateCorrectObject(int id, string? name)
    {
        var sut = new MessageBrokerExternalObject( id, name );
        Assertion.All( sut.Id.TestEquals( id ), sut.Name.TestEquals( name ) ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new MessageBrokerExternalObject( 1, "foo" );
        var result = sut.ToString();
        result.TestEquals( "Id = 1, Name = 'foo'" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenNameIsNull()
    {
        var sut = new MessageBrokerExternalObject( 1 );
        var result = sut.ToString();
        result.TestEquals( "Id = 1" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = new MessageBrokerExternalObject( 1, "foo" );
        var expected = HashCode.Combine( 1, "foo" );
        var result = sut.GetHashCode();
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0, null, 0, null, true )]
    [InlineData( 1, "foo", 1, "foo", true )]
    [InlineData( 1, "foo", 1, "Foo", false )]
    [InlineData( 1, "foo", 1, "bar", false )]
    [InlineData( 1, "foo", 2, "foo", false )]
    [InlineData( 1, "foo", 2, "bar", false )]
    public void Equals_ShouldReturnCorrectResult(int id1, string? name1, int id2, string? name2, bool expected)
    {
        var sut = new MessageBrokerExternalObject( id1, name1 );
        var other = new MessageBrokerExternalObject( id2, name2 );
        var result = sut.Equals( other );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0, null, 0, null, true )]
    [InlineData( 1, "foo", 1, "foo", true )]
    [InlineData( 1, "foo", 1, "Foo", false )]
    [InlineData( 1, "foo", 1, "bar", false )]
    [InlineData( 1, "foo", 2, "foo", false )]
    [InlineData( 1, "foo", 2, "bar", false )]
    public void EqualityOperator_ShouldReturnCorrectResult(int id1, string? name1, int id2, string? name2, bool expected)
    {
        var sut = new MessageBrokerExternalObject( id1, name1 );
        var other = new MessageBrokerExternalObject( id2, name2 );
        var result = sut == other;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0, null, 0, null, false )]
    [InlineData( 1, "foo", 1, "foo", false )]
    [InlineData( 1, "foo", 1, "Foo", true )]
    [InlineData( 1, "foo", 1, "bar", true )]
    [InlineData( 1, "foo", 2, "foo", true )]
    [InlineData( 1, "foo", 2, "bar", true )]
    public void InequalityOperator_ShouldReturnCorrectResult(int id1, string? name1, int id2, string? name2, bool expected)
    {
        var sut = new MessageBrokerExternalObject( id1, name1 );
        var other = new MessageBrokerExternalObject( id2, name2 );
        var result = sut != other;
        result.TestEquals( expected ).Go();
    }
}
