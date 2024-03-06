using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnModificationSourcesSetTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptySet()
    {
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Add_ForColumn_ShouldAddNewColumnSelfSource()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.Add( column );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) );
        }
    }

    [Fact]
    public void Add_ShouldAddNewColumnSource()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var other = schema.Objects.GetTable( "T" ).Columns.Create( "D" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.Add( new SqlColumnModificationSource<SqlColumnBuilder>( column, other ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( new SqlColumnModificationSource<SqlColumnBuilder>( column, other ) );
        }
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenColumnSourceAlreadyExists()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        var result = sut.Add( column );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            ToArray( sut ).Should().BeSequentiallyEqualTo( SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveExistingColumnSource()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        var result = sut.Remove( column );

        using ( new AssertionScope() )
        {
            result.Should().Be( SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) );
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldReturnNull_WhenColumnSourceDoesNotExist()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.Remove( column );

        using ( new AssertionScope() )
        {
            result.Should().BeNull();
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    [Fact]
    public void TryGetSource_ShouldReturnColumnSource_WhenItExists()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        var result = sut.TryGetSource( column );

        result.Should().Be( SqlColumnModificationSource<SqlColumnBuilder>.Self( column ) );
    }

    [Fact]
    public void TryGetSource_ShouldReturnNull_WhenColumnSourceDoesNotExist()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();

        var result = sut.TryGetSource( column );

        result.Should().BeNull();
    }

    [Fact]
    public void Clear_ShouldRemoveAllColumnSources()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var column = schema.Objects.CreateTable( "T" ).Columns.Create( "C" );
        var sut = SqlColumnModificationSourcesSet<SqlColumnBuilder>.Create();
        sut.Add( column );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            ToArray( sut ).Should().BeEmpty();
        }
    }

    private static SqlColumnModificationSource<SqlColumnBuilder>[] ToArray(SqlColumnModificationSourcesSet<SqlColumnBuilder> set)
    {
        var i = 0;
        var result = new SqlColumnModificationSource<SqlColumnBuilder>[set.Count];
        foreach ( var obj in set )
            result[i++] = obj;

        return result;
    }
}
