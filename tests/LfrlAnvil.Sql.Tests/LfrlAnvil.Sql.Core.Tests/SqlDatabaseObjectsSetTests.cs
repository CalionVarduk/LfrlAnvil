using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseObjectsSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

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
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Add( obj );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( obj );
        }
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenObjectAlreadyExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        var result = sut.Add( obj );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( obj );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveExistingObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        var result = sut.Remove( obj );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Remove( obj );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenObjectExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        var result = sut.Contains( obj );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Contains( obj );

        result.Should().BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllObjects()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    private static T[] ToArray<T>(SqlDatabaseObjectsSet<T> set)
        where T : SqlObjectBuilder
    {
        var i = 0;
        var result = new T[set.Count];
        foreach ( var obj in set )
            result[i++] = obj;

        return result;
    }
}
