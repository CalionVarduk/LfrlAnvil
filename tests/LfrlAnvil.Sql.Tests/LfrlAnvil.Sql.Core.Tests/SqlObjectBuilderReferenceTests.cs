using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectBuilderReferenceTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectReference()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );

        var result = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        using ( new AssertionScope() )
        {
            result.Source.Should().Be( SqlObjectBuilderReferenceSource.Create( table ) );
            result.Target.Should().BeSameAs( column );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        var result = sut.ToString();

        result.Should().Be( "[Table] common.T => [Column] common.T.C" );
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnCorrectReference()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        var result = sut.UnsafeReinterpretAs<ISqlObjectBuilder>();

        using ( new AssertionScope() )
        {
            result.Source.Should().Be( SqlObjectBuilderReferenceSource.Create( table ).UnsafeReinterpretAs<ISqlObjectBuilder>() );
            result.Target.Should().BeSameAs( column );
        }
    }

    [Fact]
    public void SqlObjectBuilderReferenceConversionOperator_ShouldReturnCorrectReference()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        SqlObjectBuilderReference<ISqlObjectBuilder> result = sut;

        using ( new AssertionScope() )
        {
            result.Source.Should().Be( SqlObjectBuilderReferenceSource.Create( table ).UnsafeReinterpretAs<ISqlObjectBuilder>() );
            result.Target.Should().BeSameAs( column );
        }
    }
}
