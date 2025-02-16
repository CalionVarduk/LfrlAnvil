using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseObjectsSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Add( obj );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut ).TestSequence( [ obj ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenObjectAlreadyExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        var result = sut.Add( obj );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut ).TestSequence( [ obj ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveExistingObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        var result = sut.Remove( obj );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Remove( obj );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenObjectExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        var result = sut.Contains( obj );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Contains( obj );

        result.TestFalse().Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllObjects()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( obj );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Pure]
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
