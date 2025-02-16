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

        Assertion.All(
                sut.Name.TestEquals( "INTEGER" ),
                sut.Value.TestEquals( SqliteType.Integer ),
                sut.DbType.TestEquals( DbType.Int64 ),
                sut.Dialect.TestRefEquals( SqliteDialect.Instance ),
                (( ISqlDataType )sut).Parameters.ToArray().TestEmpty(),
                (( ISqlDataType )sut).ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Real_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Real;

        Assertion.All(
                sut.Name.TestEquals( "REAL" ),
                sut.Value.TestEquals( SqliteType.Real ),
                sut.DbType.TestEquals( DbType.Double ),
                sut.Dialect.TestRefEquals( SqliteDialect.Instance ),
                (( ISqlDataType )sut).Parameters.ToArray().TestEmpty(),
                (( ISqlDataType )sut).ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Text_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Text;

        Assertion.All(
                sut.Name.TestEquals( "TEXT" ),
                sut.Value.TestEquals( SqliteType.Text ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( SqliteDialect.Instance ),
                (( ISqlDataType )sut).Parameters.ToArray().TestEmpty(),
                (( ISqlDataType )sut).ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Blob_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Blob;

        Assertion.All(
                sut.Name.TestEquals( "BLOB" ),
                sut.Value.TestEquals( SqliteType.Blob ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( SqliteDialect.Instance ),
                (( ISqlDataType )sut).Parameters.ToArray().TestEmpty(),
                (( ISqlDataType )sut).ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Any_ShouldHaveCorrectProperties()
    {
        var sut = SqliteDataType.Any;

        Assertion.All(
                sut.Name.TestEquals( "ANY" ),
                sut.Value.TestEquals( ( SqliteType )0 ),
                sut.DbType.TestEquals( DbType.Object ),
                sut.Dialect.TestRefEquals( SqliteDialect.Instance ),
                (( ISqlDataType )sut).Parameters.ToArray().TestEmpty(),
                (( ISqlDataType )sut).ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }
}
