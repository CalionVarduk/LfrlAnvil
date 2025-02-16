using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new SqliteDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnInteger()
    {
        var result = _sut.GetBool();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetInt8_ShouldReturnInteger()
    {
        var result = _sut.GetInt8();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetInt16_ShouldReturnInteger()
    {
        var result = _sut.GetInt16();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetInt32_ShouldReturnInteger()
    {
        var result = _sut.GetInt32();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetInt64_ShouldReturnInteger()
    {
        var result = _sut.GetInt64();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetUInt8_ShouldReturnInteger()
    {
        var result = _sut.GetUInt8();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetUInt16_ShouldReturnInteger()
    {
        var result = _sut.GetUInt16();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetUInt32_ShouldReturnInteger()
    {
        var result = _sut.GetUInt32();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetUInt64_ShouldReturnInteger()
    {
        var result = _sut.GetUInt64();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetFloat_ShouldReturnReal()
    {
        var result = _sut.GetFloat();
        result.TestRefEquals( SqliteDataType.Real ).Go();
    }

    [Fact]
    public void GetDouble_ShouldReturnReal()
    {
        var result = _sut.GetDouble();
        result.TestRefEquals( SqliteDataType.Real ).Go();
    }

    [Fact]
    public void GetDecimal_ShouldReturnText()
    {
        var result = _sut.GetDecimal();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetDecimal_WithParameters_ShouldReturnText()
    {
        var result = _sut.GetDecimal( Fixture.Create<int>(), Fixture.Create<int>() );
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetGuid_ShouldReturnBlob()
    {
        var result = _sut.GetGuid();
        result.TestRefEquals( SqliteDataType.Blob ).Go();
    }

    [Fact]
    public void GetString_ShouldReturnText()
    {
        var result = _sut.GetString();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetString_WithLength_ShouldReturnText()
    {
        var result = _sut.GetString( Fixture.Create<int>() );
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetFixedString_ShouldReturnText()
    {
        var result = _sut.GetFixedString();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetFixedString_WithLength_ShouldReturnText()
    {
        var result = _sut.GetFixedString( Fixture.Create<int>() );
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetTimestamp_ShouldReturnInteger()
    {
        var result = _sut.GetTimestamp();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetUtcDateTime_ShouldReturnText()
    {
        var result = _sut.GetUtcDateTime();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetDateTime_ShouldReturnText()
    {
        var result = _sut.GetDateTime();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnInteger()
    {
        var result = _sut.GetTimeSpan();
        result.TestRefEquals( SqliteDataType.Integer ).Go();
    }

    [Fact]
    public void GetDate_ShouldReturnText()
    {
        var result = _sut.GetDate();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetTime_ShouldReturnText()
    {
        var result = _sut.GetTime();
        result.TestRefEquals( SqliteDataType.Text ).Go();
    }

    [Fact]
    public void GetBinary_ShouldReturnBlob()
    {
        var result = _sut.GetBinary();
        result.TestRefEquals( SqliteDataType.Blob ).Go();
    }

    [Fact]
    public void GetBinary_WithLength_ShouldReturnBlob()
    {
        var result = _sut.GetBinary( Fixture.Create<int>() );
        result.TestRefEquals( SqliteDataType.Blob ).Go();
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBlob()
    {
        var result = _sut.GetFixedBinary();
        result.TestRefEquals( SqliteDataType.Blob ).Go();
    }

    [Fact]
    public void GetFixedBinary_WithLength_ShouldReturnBlob()
    {
        var result = _sut.GetFixedBinary( Fixture.Create<int>() );
        result.TestRefEquals( SqliteDataType.Blob ).Go();
    }

    [Fact]
    public void GetAny_ShouldReturnAny()
    {
        var result = (( SqliteDataTypeProvider )_sut).GetAny();
        result.TestRefEquals( SqliteDataType.Any ).Go();
    }
}
