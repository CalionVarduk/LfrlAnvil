using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseNamedSchemaObjectsSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_ShouldAddNewObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            ToArray( sut )
                .Should()
                .BeSequentiallyEqualTo( new SqlNamedSchemaObject<SqlObjectBuilder>( SqlSchemaObjectName.Create( "foo", "bar" ), obj ) );
        }
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenNameAlreadyExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        var result = sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj.Objects.CreateTable( "T" ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            ToArray( sut )
                .Should()
                .BeSequentiallyEqualTo( new SqlNamedSchemaObject<SqlObjectBuilder>( SqlSchemaObjectName.Create( "foo", "bar" ), obj ) );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveExistingObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        var result = sut.Remove( SqlSchemaObjectName.Create( "foo", "bar" ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( obj );
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldReturnNull_WhenNameDoesNotExist()
    {
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Remove( SqlSchemaObjectName.Create( "foo", "bar" ) );

        using ( new AssertionScope() )
        {
            result.Should().BeNull();
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void TryGetObject_ShouldReturnObject_WhenNameExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        var result = sut.TryGetObject( SqlSchemaObjectName.Create( "foo", "bar" ) );

        result.Should().BeSameAs( obj );
    }

    [Fact]
    public void TryGetObject_ShouldReturnNull_WhenNameDoesNotExist()
    {
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        var result = sut.TryGetObject( SqlSchemaObjectName.Create( "foo", "bar" ) );
        result.Should().BeNull();
    }

    [Fact]
    public void Clear_ShouldRemoveAllObjects()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    private static SqlNamedSchemaObject<T>[] ToArray<T>(SqlDatabaseNamedSchemaObjectsSet<T> set)
        where T : SqlObjectBuilder
    {
        var i = 0;
        var result = new SqlNamedSchemaObject<T>[set.Count];
        foreach ( var obj in set )
            result[i++] = obj;

        return result;
    }
}
