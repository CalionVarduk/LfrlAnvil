using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectBuilderReferenceSourceTests : TestsBase
{
    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void Create_ShouldReturnCorrectReferenceSource(string? property)
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var result = SqlObjectBuilderReferenceSource.Create( table, property );

        using ( new AssertionScope() )
        {
            result.Object.Should().BeSameAs( table );
            result.Property.Should().BeSameAs( property );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutProperty()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table );

        var result = sut.ToString();

        result.Should().Be( "[Table] common.T" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithProperty()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = sut.ToString();

        result.Should().Be( "[Table] common.T (foo)" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var expected = HashCode.Combine( table.Id, "foo" );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void WithProperty_ShouldReturnReferenceSourceWithNewProperty()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = sut.WithProperty( "bar" );

        using ( new AssertionScope() )
        {
            result.Object.Should().BeSameAs( table );
            result.Property.Should().Be( "bar" );
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void UnsafeReinterpretAs_ShouldReturnCorrectReferenceSource(string? property)
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, property );

        var result = sut.UnsafeReinterpretAs<ISqlObjectBuilder>();

        using ( new AssertionScope() )
        {
            result.Object.Should().BeSameAs( table );
            result.Property.Should().BeSameAs( property );
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void SqlObjectBuilderReferenceConversionOperator_ShouldReturnCorrectReferenceSource(string? property)
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, property );

        SqlObjectBuilderReferenceSource<ISqlObjectBuilder> result = sut;

        using ( new AssertionScope() )
        {
            result.Object.Should().BeSameAs( table );
            result.Property.Should().BeSameAs( property );
        }
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenObjectAndPropertyAreTheSame()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = a == b;

        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenObjectIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var other = table.Database.Schemas.Default;
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( other, "foo" );

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenPropertyIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "bar" );

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenObjectAndPropertyAreTheSame()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = a != b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenObjectIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var other = table.Database.Schemas.Default;
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( other, "foo" );

        var result = a != b;

        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenPropertyIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "bar" );

        var result = a != b;

        result.Should().BeTrue();
    }
}
