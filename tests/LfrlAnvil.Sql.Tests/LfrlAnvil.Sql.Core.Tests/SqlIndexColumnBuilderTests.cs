using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlIndexColumnBuilderTests : TestsBase
{
    [Fact]
    public void CreateAsc_ShouldCreateWithAscOrdering()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlIndexColumnBuilder.CreateAsc( column );

        using ( new AssertionScope() )
        {
            sut.Column.Should().BeSameAs( column );
            sut.Ordering.Should().BeSameAs( OrderBy.Asc );
        }
    }

    [Fact]
    public void CreateDesc_ShouldCreateWithDescOrdering()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlIndexColumnBuilder.CreateDesc( column );

        using ( new AssertionScope() )
        {
            sut.Column.Should().BeSameAs( column );
            sut.Ordering.Should().BeSameAs( OrderBy.Desc );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );

        var result = sut.ToString();

        result.Should().Be( "[Column] common.T.C ASC" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );
        var expected = HashCode.Combine( column.Id, OrderBy.Asc );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnSelfWithProvidedColumnType()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );

        var result = sut.UnsafeReinterpretAs<SqlColumnBuilder>();

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut.Column );
            result.Ordering.Should().BeSameAs( sut.Ordering );
        }
    }

    [Fact]
    public void SqlIndexColumnBuilderConversionOperator_ShouldReturnSelfWithBaseColumnType()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );

        SqlIndexColumnBuilder<ISqlColumnBuilder> result = sut;

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut.Column );
            result.Ordering.Should().BeSameAs( sut.Ordering );
        }
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenColumnAndOrderingAreTheSame()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var a = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );
        var b = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );

        var result = a == b;

        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenOrderingIsDifferent()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var a = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );
        var b = SqlIndexColumnBuilder.Create( column, OrderBy.Desc );

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenColumnIsDifferent()
    {
        var c1 = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var c2 = c1.Table.Columns.Create( "D" );
        var a = SqlIndexColumnBuilder.Create( c1, OrderBy.Asc );
        var b = SqlIndexColumnBuilder.Create( c2, OrderBy.Asc );

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenColumnAndOrderingAreTheSame()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var a = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );
        var b = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );

        var result = a != b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenOrderingIsDifferent()
    {
        var column = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var a = SqlIndexColumnBuilder.Create( column, OrderBy.Asc );
        var b = SqlIndexColumnBuilder.Create( column, OrderBy.Desc );

        var result = a != b;

        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenColumnIsDifferent()
    {
        var c1 = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var c2 = c1.Table.Columns.Create( "D" );
        var a = SqlIndexColumnBuilder.Create( c1, OrderBy.Asc );
        var b = SqlIndexColumnBuilder.Create( c2, OrderBy.Asc );

        var result = a != b;

        result.Should().BeTrue();
    }
}
