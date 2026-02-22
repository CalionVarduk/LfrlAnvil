using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectBuilderReferenceCollectionTests : TestsBase
{
    [Fact]
    public void Object_ShouldHaveEmptyCollection_WhenItNotHaveAnyReferences()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Object_ShouldHaveCorrectCollection_WhenItHasReferences()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var r1 = SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ) );
        var r2 = SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "U" ) );
        SqlDatabaseBuilderMock.AddReference( obj, r1 );
        SqlDatabaseBuilderMock.AddReference( obj, r2 );
        var sut = obj.ReferencingObjects;

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ SqlObjectBuilderReference.Create( r1, obj ), SqlObjectBuilderReference.Create( r2, obj ) ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void Contains_ShouldReturnFalse_WhenReferenceDoesNotExist(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        var result = sut.Contains( SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ), property ) );

        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void Contains_ShouldReturnTrue_WhenReferenceExists(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = obj.Objects.CreateTable( "T" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table, "foo" ) );
        var sut = obj.ReferencingObjects;

        var result = sut.Contains( SqlObjectBuilderReferenceSource.Create( table, property ) );

        result.TestTrue().Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void GetReference_ShouldThrowSqlObjectBuilderException_WhenReferenceDoesNotExist(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        var action
            = Lambda.Of( () => sut.GetReference( SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ), property ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void GetReference_ShouldReturnCorrectResult_WhenReferenceExists(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = obj.Objects.CreateTable( "T" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table, "foo" ) );
        var sut = obj.ReferencingObjects;

        var result = sut.GetReference( SqlObjectBuilderReferenceSource.Create( table, property ) );

        Assertion.All(
                result.Source.TestEquals( SqlObjectBuilderReferenceSource.Create( table, property ) ),
                result.Target.TestRefEquals( obj ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void TryGetReference_ShouldReturnNull_WhenReferenceDoesNotExist(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        var result = sut.TryGetReference( SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ), property ) );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void TryGetReference_ShouldReturnCorrectResult_WhenReferenceExists(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = obj.Objects.CreateTable( "T" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table, "foo" ) );
        var sut = obj.ReferencingObjects;

        var result = sut.TryGetReference( SqlObjectBuilderReferenceSource.Create( table, property ) );

        Assertion.All(
                result.TestNotNull(),
                (result?.Source).TestEquals( SqlObjectBuilderReferenceSource.Create( table, property ) ),
                (result?.Target).TestRefEquals( obj ) )
            .Go();
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnCorrectCollection()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var r1 = SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ) );
        var r2 = SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "U" ) );
        SqlDatabaseBuilderMock.AddReference( obj, r1 );
        SqlDatabaseBuilderMock.AddReference( obj, r2 );
        var sut = obj.ReferencingObjects;

        var result = sut.UnsafeReinterpretAs<ISqlObjectBuilder>();

        Assertion.All(
                result.Count.TestEquals( sut.Count ),
                result.TestSetEqual(
                [
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r1, obj ),
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r2, obj )
                ] ) )
            .Go();
    }

    [Fact]
    public void SqlObjectBuilderReferenceCollectionConversionOperator_ShouldReturnCorrectCollection()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var r1 = SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ) );
        var r2 = SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "U" ) );
        SqlDatabaseBuilderMock.AddReference( obj, r1 );
        SqlDatabaseBuilderMock.AddReference( obj, r2 );
        var sut = obj.ReferencingObjects;

        SqlObjectBuilderReferenceCollection<ISqlObjectBuilder> result = sut;

        Assertion.All(
                result.Count.TestEquals( sut.Count ),
                result.TestSetEqual(
                [
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r1, obj ),
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r2, obj )
                ] ) )
            .Go();
    }
}
