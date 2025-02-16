using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseNamedSchemaObjectsSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut )
                    .TestSequence( [ new SqlNamedSchemaObject<SqlObjectBuilder>( SqlSchemaObjectName.Create( "foo", "bar" ), obj ) ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenNameAlreadyExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        var result = sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj.Objects.CreateTable( "T" ) );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut )
                    .TestSequence( [ new SqlNamedSchemaObject<SqlObjectBuilder>( SqlSchemaObjectName.Create( "foo", "bar" ), obj ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveExistingObject()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        var result = sut.Remove( SqlSchemaObjectName.Create( "foo", "bar" ) );

        Assertion.All(
                result.TestRefEquals( obj ),
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnNull_WhenNameDoesNotExist()
    {
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();

        var result = sut.Remove( SqlSchemaObjectName.Create( "foo", "bar" ) );

        Assertion.All(
                result.TestNull(),
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void TryGetObject_ShouldReturnObject_WhenNameExists()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        var result = sut.TryGetObject( SqlSchemaObjectName.Create( "foo", "bar" ) );

        result.TestRefEquals( obj ).Go();
    }

    [Fact]
    public void TryGetObject_ShouldReturnNull_WhenNameDoesNotExist()
    {
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        var result = sut.TryGetObject( SqlSchemaObjectName.Create( "foo", "bar" ) );
        result.TestNull().Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllObjects()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = SqlDatabaseNamedSchemaObjectsSet<SqlObjectBuilder>.Create();
        sut.Add( SqlSchemaObjectName.Create( "foo", "bar" ), obj );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Pure]
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
