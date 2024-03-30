using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new SqliteDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnInteger()
    {
        var result = _sut.GetBool();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt8_ShouldReturnInteger()
    {
        var result = _sut.GetInt8();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt16_ShouldReturnInteger()
    {
        var result = _sut.GetInt16();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt32_ShouldReturnInteger()
    {
        var result = _sut.GetInt32();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt64_ShouldReturnInteger()
    {
        var result = _sut.GetInt64();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt8_ShouldReturnInteger()
    {
        var result = _sut.GetUInt8();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt16_ShouldReturnInteger()
    {
        var result = _sut.GetUInt16();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt32_ShouldReturnInteger()
    {
        var result = _sut.GetUInt32();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt64_ShouldReturnInteger()
    {
        var result = _sut.GetUInt64();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetFloat_ShouldReturnReal()
    {
        var result = _sut.GetFloat();
        result.Should().BeSameAs( SqliteDataType.Real );
    }

    [Fact]
    public void GetDouble_ShouldReturnReal()
    {
        var result = _sut.GetDouble();
        result.Should().BeSameAs( SqliteDataType.Real );
    }

    [Fact]
    public void GetDecimal_ShouldReturnText()
    {
        var result = _sut.GetDecimal();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetDecimal_WithParameters_ShouldReturnText()
    {
        var result = _sut.GetDecimal( Fixture.Create<int>(), Fixture.Create<int>() );
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetGuid_ShouldReturnBlob()
    {
        var result = _sut.GetGuid();
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Fact]
    public void GetString_ShouldReturnText()
    {
        var result = _sut.GetString();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetString_WithLength_ShouldReturnText()
    {
        var result = _sut.GetString( Fixture.Create<int>() );
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetFixedString_ShouldReturnText()
    {
        var result = _sut.GetFixedString();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetFixedString_WithLength_ShouldReturnText()
    {
        var result = _sut.GetFixedString( Fixture.Create<int>() );
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetTimestamp_ShouldReturnInteger()
    {
        var result = _sut.GetTimestamp();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUtcDateTime_ShouldReturnText()
    {
        var result = _sut.GetUtcDateTime();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetDateTime_ShouldReturnText()
    {
        var result = _sut.GetDateTime();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnInteger()
    {
        var result = _sut.GetTimeSpan();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetDate_ShouldReturnText()
    {
        var result = _sut.GetDate();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetTime_ShouldReturnText()
    {
        var result = _sut.GetTime();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetBinary_ShouldReturnBlob()
    {
        var result = _sut.GetBinary();
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Fact]
    public void GetBinary_WithLength_ShouldReturnBlob()
    {
        var result = _sut.GetBinary( Fixture.Create<int>() );
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBlob()
    {
        var result = _sut.GetFixedBinary();
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Fact]
    public void GetFixedBinary_WithLength_ShouldReturnBlob()
    {
        var result = _sut.GetFixedBinary( Fixture.Create<int>() );
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Fact]
    public void GetAny_ShouldReturnAny()
    {
        var result = ((SqliteDataTypeProvider)_sut).GetAny();
        result.Should().BeSameAs( SqliteDataType.Any );
    }
}
