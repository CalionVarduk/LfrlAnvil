using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectBuilderReferenceTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectReference()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );

        var result = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        Assertion.All(
                result.Source.TestEquals( SqlObjectBuilderReferenceSource.Create( table ) ),
                result.Target.TestRefEquals( column ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        var result = sut.ToString();

        result.TestEquals( "[Table] common.T => [Column] common.T.C" ).Go();
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnCorrectReference()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        var result = sut.UnsafeReinterpretAs<ISqlObjectBuilder>();

        Assertion.All(
                result.Source.TestEquals( SqlObjectBuilderReferenceSource.Create( table ).UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                result.Target.TestRefEquals( column ) )
            .Go();
    }

    [Fact]
    public void SqlObjectBuilderReferenceConversionOperator_ShouldReturnCorrectReference()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( table ), column );

        SqlObjectBuilderReference<ISqlObjectBuilder> result = sut;

        Assertion.All(
                result.Source.TestEquals( SqlObjectBuilderReferenceSource.Create( table ).UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                result.Target.TestRefEquals( column ) )
            .Go();
    }
}
