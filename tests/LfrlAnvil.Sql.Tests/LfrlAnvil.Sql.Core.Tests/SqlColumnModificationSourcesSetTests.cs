using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnModificationSourcesSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Add_ForColumn_ShouldAddNewColumnSelfSource()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.Add( column );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut ).TestSequence( [ SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewColumnSource()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var other = schema.Objects.GetTable( "T" ).Columns.Create( "D" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.Add( new SqlColumnModificationSource<SqlColumnBuilder>( column, other ) );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut ).TestSequence( [ new SqlColumnModificationSource<SqlColumnBuilder>( column, other ) ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenColumnSourceAlreadyExists()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        var result = sut.Add( column );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                ToArray( sut ).TestSequence( [ SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveExistingColumnSource()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        var result = sut.Remove( column );

        Assertion.All(
                result.TestEquals( SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) ),
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnNull_WhenColumnSourceDoesNotExist()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.Remove( column );

        Assertion.All(
                result.TestNull(),
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Fact]
    public void TryGetSource_ShouldReturnColumnSource_WhenItExists()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        var result = sut.TryGetSource( column );

        result.TestEquals( SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) ).Go();
    }

    [Fact]
    public void TryGetSource_ShouldReturnNull_WhenColumnSourceDoesNotExist()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.TryGetSource( column );

        result.TestNull().Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllColumnSources()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                ToArray( sut ).TestEmpty() )
            .Go();
    }

    [Pure]
    private static SqlColumnModificationSource<SqlColumnBuilder>[] ToArray(SqlColumnModificationSourcesSet<SqlColumnBuilder> set)
    {
        var i = 0;
        var result = new SqlColumnModificationSource<SqlColumnBuilder>[set.Count];
        foreach ( var obj in set )
            result[i++] = obj;

        return result;
    }
}
