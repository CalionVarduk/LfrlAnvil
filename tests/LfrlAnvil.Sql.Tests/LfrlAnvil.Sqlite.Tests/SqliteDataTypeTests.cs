using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDataTypeTests : TestsBase
{
    [Fact]
    public void Integer_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Integer;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "INTEGER" );
            sut.Value.Should().Be( SqliteType.Integer );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
        }
    }

    [Fact]
    public void Real_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Real;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "REAL" );
            sut.Value.Should().Be( SqliteType.Real );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
        }
    }

    [Fact]
    public void Text_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Text;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TEXT" );
            sut.Value.Should().Be( SqliteType.Text );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
        }
    }

    [Fact]
    public void Blob_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Blob;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BLOB" );
            sut.Value.Should().Be( SqliteType.Blob );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
        }
    }

    [Fact]
    public void Any_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Any;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "ANY" );
            sut.Value.Should().Be( 0 );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
        }
    }
}
