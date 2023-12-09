using System.Data;
using LfrlAnvil.Sql;
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
            sut.DbType.Should().Be( DbType.Int64 );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            ((ISqlDataType)sut).Parameters.ToArray().Should().BeEmpty();
            ((ISqlDataType)sut).ParameterDefinitions.ToArray().Should().BeEmpty();
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
            sut.DbType.Should().Be( DbType.Double );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            ((ISqlDataType)sut).Parameters.ToArray().Should().BeEmpty();
            ((ISqlDataType)sut).ParameterDefinitions.ToArray().Should().BeEmpty();
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
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            ((ISqlDataType)sut).Parameters.ToArray().Should().BeEmpty();
            ((ISqlDataType)sut).ParameterDefinitions.ToArray().Should().BeEmpty();
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
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            ((ISqlDataType)sut).Parameters.ToArray().Should().BeEmpty();
            ((ISqlDataType)sut).ParameterDefinitions.ToArray().Should().BeEmpty();
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
            sut.DbType.Should().Be( DbType.Object );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            ((ISqlDataType)sut).Parameters.ToArray().Should().BeEmpty();
            ((ISqlDataType)sut).ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }
}
