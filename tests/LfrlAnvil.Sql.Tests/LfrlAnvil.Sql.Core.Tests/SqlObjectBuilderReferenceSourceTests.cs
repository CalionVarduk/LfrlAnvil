using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

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

        Assertion.All(
                result.Object.TestRefEquals( table ),
                result.Property.TestRefEquals( property ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithoutProperty()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table );

        var result = sut.ToString();

        result.TestEquals( "[Table] common.T" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WithProperty()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = sut.ToString();

        result.TestEquals( "[Table] common.T (foo)" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var expected = HashCode.Combine( table.Id, "foo" );

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void WithProperty_ShouldReturnReferenceSourceWithNewProperty()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = sut.WithProperty( "bar" );

        Assertion.All(
                result.Object.TestRefEquals( table ),
                result.Property.TestEquals( "bar" ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void UnsafeReinterpretAs_ShouldReturnCorrectReferenceSource(string? property)
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, property );

        var result = sut.UnsafeReinterpretAs<ISqlObjectBuilder>();

        Assertion.All(
                result.Object.TestRefEquals( table ),
                result.Property.TestRefEquals( property ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void SqlObjectBuilderReferenceConversionOperator_ShouldReturnCorrectReferenceSource(string? property)
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = SqlObjectBuilderReferenceSource.Create( table, property );

        SqlObjectBuilderReferenceSource<ISqlObjectBuilder> result = sut;

        Assertion.All(
                result.Object.TestRefEquals( table ),
                result.Property.TestRefEquals( property ) )
            .Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenObjectAndPropertyAreTheSame()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = a == b;

        result.TestTrue().Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenObjectIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var other = table.Database.Schemas.Default;
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( other, "foo" );

        var result = a == b;

        result.TestFalse().Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenPropertyIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "bar" );

        var result = a == b;

        result.TestFalse().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenObjectAndPropertyAreTheSame()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "foo" );

        var result = a != b;

        result.TestFalse().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenObjectIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var other = table.Database.Schemas.Default;
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( other, "foo" );

        var result = a != b;

        result.TestTrue().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenPropertyIsDifferent()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var a = SqlObjectBuilderReferenceSource.Create( table, "foo" );
        var b = SqlObjectBuilderReferenceSource.Create( table, "bar" );

        var result = a != b;

        result.TestTrue().Go();
    }
}
