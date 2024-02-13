using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseNamedObjectsSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();

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
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Add( "foo", obj );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( new SqlNamedObject<SqlObjectBuilder>( "foo", obj ) );
        }
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenNameAlreadyExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( "foo", obj );

        var result = sut.Add( "foo", obj.Objects.CreateTable( "T" ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( new SqlNamedObject<SqlObjectBuilder>( "foo", obj ) );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveExistingObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( "foo", obj );

        var result = sut.Remove( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Remove( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void TryGetObject_ShouldReturnObject_WhenNameExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( "foo", obj );

        var result = sut.TryGetObject( "foo" );

        result.Should().BeSameAs( obj );
    }

    [Fact]
    public void TryGetObject_ShouldReturnNul_WhenNameDoesNotExist()
    {
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();
        var result = sut.TryGetObject( "foo" );
        result.Should().BeNull();
    }

    [Fact]
    public void Clear_ShouldRemoveAllObjects()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( "foo", obj );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    private static SqlNamedObject<T>[] ToArray<T>(SqlDatabaseNamedObjectsSet<T> set)
        where T : SqlObjectBuilder
    {
        var i = 0;
        var result = new SqlNamedObject<T>[set.Count];
        foreach ( var obj in set )
            result[i++] = obj;

        return result;
    }
}
