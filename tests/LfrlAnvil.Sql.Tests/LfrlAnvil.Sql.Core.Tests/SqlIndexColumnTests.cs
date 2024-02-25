using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlIndexColumnTests : TestsBase
{
    [Fact]
    public void CreateAsc_ShouldCreateWithAscOrdering()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );

        var sut = SqlIndexColumn.CreateAsc( column );

        using ( new AssertionScope() )
        {
            sut.Column.Should().BeSameAs( column );
            sut.Ordering.Should().BeSameAs( OrderBy.Asc );
        }
    }

    [Fact]
    public void CreateDesc_ShouldCreateWithDescOrdering()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );

        var sut = SqlIndexColumn.CreateDesc( column );

        using ( new AssertionScope() )
        {
            sut.Column.Should().BeSameAs( column );
            sut.Ordering.Should().BeSameAs( OrderBy.Desc );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );

        var sut = SqlIndexColumn.Create( column, OrderBy.Asc );

        var result = sut.ToString();

        result.Should().Be( "[Column] common.T.C ASC" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );

        var sut = SqlIndexColumn.Create( column, OrderBy.Asc );
        var expected = HashCode.Combine( column, OrderBy.Asc );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnSelfWithProvidedColumnType()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );

        var sut = SqlIndexColumn.Create( column, OrderBy.Asc );

        var result = sut.UnsafeReinterpretAs<SqlColumn>();

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut.Column );
            result.Ordering.Should().BeSameAs( sut.Ordering );
        }
    }

    [Fact]
    public void SqlIndexColumnBuilderConversionOperator_ShouldReturnSelfWithBaseColumnType()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );

        var sut = SqlIndexColumn.Create( column, OrderBy.Asc );

        SqlIndexColumn<ISqlColumn> result = sut;

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut.Column );
            result.Ordering.Should().BeSameAs( sut.Ordering );
        }
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenColumnAndOrderingAreTheSame()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );
        var a = SqlIndexColumn.Create( column, OrderBy.Asc );
        var b = SqlIndexColumn.Create( column, OrderBy.Asc );

        var result = a == b;

        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenOrderingIsDifferent()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );
        var a = SqlIndexColumn.Create( column, OrderBy.Asc );
        var b = SqlIndexColumn.Create( column, OrderBy.Desc );

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenColumnIsDifferent()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Columns.Create( "D" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var c1 = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );
        var c2 = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "D" );
        var a = SqlIndexColumn.Create( c1, OrderBy.Asc );
        var b = SqlIndexColumn.Create( c2, OrderBy.Asc );

        var result = a == b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenColumnAndOrderingAreTheSame()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );
        var a = SqlIndexColumn.Create( column, OrderBy.Asc );
        var b = SqlIndexColumn.Create( column, OrderBy.Asc );

        var result = a != b;

        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenOrderingIsDifferent()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var column = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );
        var a = SqlIndexColumn.Create( column, OrderBy.Asc );
        var b = SqlIndexColumn.Create( column, OrderBy.Desc );

        var result = a != b;

        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenColumnIsDifferent()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Columns.Create( "D" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var c1 = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "C" );
        var c2 = db.Schemas.Default.Objects.GetTable( "T" ).Columns.Get( "D" );
        var a = SqlIndexColumn.Create( c1, OrderBy.Asc );
        var b = SqlIndexColumn.Create( c2, OrderBy.Asc );

        var result = a != b;

        result.Should().BeTrue();
    }
}
