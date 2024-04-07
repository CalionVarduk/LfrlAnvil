using LfrlAnvil.Functional;

namespace LfrlAnvil.Sql.Tests;

public class SqlDialectTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithProvidedName()
    {
        var name = Fixture.Create<string>();
        var sut = new SqlDialect( name );
        sut.Name.Should().BeSameAs( name );
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        var action = Lambda.Of( () => new SqlDialect( string.Empty ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        var name = Fixture.Create<string>();
        var sut = new SqlDialect( name );

        var result = sut.ToString();

        result.Should().BeSameAs( name );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var name = Fixture.Create<string>();
        var sut = new SqlDialect( name );

        var result = sut.GetHashCode();

        result.Should().Be( name.GetHashCode() );
    }

    [Theory]
    [InlineData( "foo", "foo", true )]
    [InlineData( "foo", "bar", false )]
    public void Equals_ShouldUseNameForComparison(string a, string b, bool expected)
    {
        var sut = new SqlDialect( a );
        var other = new SqlDialect( b );

        var result = sut.Equals( other );

        result.Should().Be( expected );
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        var sut = new SqlDialect( Fixture.Create<string>() );
        var result = sut.Equals( ( SqlDialect? )null );
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( "foo", "foo", true )]
    [InlineData( "foo", "bar", false )]
    public void EqualityOperator_ShouldUseNameForComparison(string a, string b, bool expected)
    {
        var sut = new SqlDialect( a );
        var other = new SqlDialect( b );

        var result = sut == other;

        result.Should().Be( expected );
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenFirstIsNotNullAndSecondIsNull()
    {
        var sut = new SqlDialect( Fixture.Create<string>() );
        var result = sut == ( SqlDialect? )null;
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenFirstIsNullAndSecondIsNotNull()
    {
        var sut = new SqlDialect( Fixture.Create<string>() );
        var result = ( SqlDialect? )null == sut;
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenFirstIsNullAndSecondIsNull()
    {
        var result = ( SqlDialect? )null == null;
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( "foo", "foo", false )]
    [InlineData( "foo", "bar", true )]
    public void InequalityOperator_ShouldUseNameForComparison(string a, string b, bool expected)
    {
        var sut = new SqlDialect( a );
        var other = new SqlDialect( b );

        var result = sut != other;

        result.Should().Be( expected );
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenFirstIsNotNullAndSecondIsNull()
    {
        var sut = new SqlDialect( Fixture.Create<string>() );
        var result = sut != ( SqlDialect? )null;
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenFirstIsNullAndSecondIsNotNull()
    {
        var sut = new SqlDialect( Fixture.Create<string>() );
        var result = ( SqlDialect? )null != sut;
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenFirstIsNullAndSecondIsNull()
    {
        var result = ( SqlDialect? )null != null;
        result.Should().BeFalse();
    }
}
