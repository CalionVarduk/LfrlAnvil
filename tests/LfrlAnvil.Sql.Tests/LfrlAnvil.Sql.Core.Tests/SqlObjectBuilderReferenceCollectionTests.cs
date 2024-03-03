using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectBuilderReferenceCollectionTests : TestsBase
{
    [Fact]
    public void Object_ShouldHaveEmptyCollection_WhenItNotHaveAnyReferences()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( SqlObjectBuilderReference.Create( r1, obj ), SqlObjectBuilderReference.Create( r2, obj ) );
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void Contains_ShouldReturnFalse_WhenReferenceDoesNotExist(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        var result = sut.Contains( SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ), property ) );

        result.Should().BeFalse();
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

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void GetReference_ShouldThrowSqlObjectBuilderException_WhenReferenceDoesNotExist(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        var action = Lambda.Of(
            () => sut.GetReference( SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ), property ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
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

        using ( new AssertionScope() )
        {
            result.Source.Should().Be( SqlObjectBuilderReferenceSource.Create( table, property ) );
            result.Target.Should().BeSameAs( obj );
        }
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void TryGetReference_ShouldReturnNull_WhenReferenceDoesNotExist(string? property)
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = obj.ReferencingObjects;

        var result = sut.TryGetReference( SqlObjectBuilderReferenceSource.Create( obj.Objects.CreateTable( "T" ), property ) );

        result.Should().BeNull();
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

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.Source).Should().Be( SqlObjectBuilderReferenceSource.Create( table, property ) );
            (result?.Target).Should().BeSameAs( obj );
        }
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

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( sut.Count );
            result.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r1, obj ),
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r2, obj ) );
        }
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

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( sut.Count );
            result.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r1, obj ),
                    SqlObjectBuilderReference.Create<ISqlObjectBuilder>( r2, obj ) );
        }
    }
}
