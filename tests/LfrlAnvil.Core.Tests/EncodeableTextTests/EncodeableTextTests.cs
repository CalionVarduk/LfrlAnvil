using System.Text;

namespace LfrlAnvil.Tests.EncodeableTextTests;

public class EncodeableTextTests : TestsBase
{
    [Fact]
    public void Default_ShouldContainEmptyString()
    {
        var sut = default( EncodeableText );
        Assertion.All(
                sut.ByteCount.TestEquals( 0 ),
                sut.Value.TestEmpty(),
                sut.Encoding.TestRefEquals( Encoding.UTF8 ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnValidResult_WhenEncoderSuccessfullyRecoversByteCount()
    {
        var result = EncodeableText.Create( Encoding.Unicode, "foobar" );

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.ByteCount.TestEquals( 12 ),
                result.Value.Value.ToString().TestEquals( "foobar" ),
                result.Value.Encoding.TestRefEquals( Encoding.Unicode ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnInvalidResult_WhenEncoderFailsToRecoverByteCount()
    {
        var result = EncodeableText.Create( new InvalidEncoding(), "foobar" );
        result.Exception.TestType().Exact<NotSupportedException>().Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = EncodeableText.Create( Encoding.UTF8, "foobar" ).Value;
        var result = sut.ToString();
        result.TestEquals( "[Encoding: Unicode (UTF-8), ByteCount: 6] 'foobar'" ).Go();
    }

    [Fact]
    public void Encode_ShouldReturnValidResult_WhenEncoderPerformsOperationSuccessfully()
    {
        var target = new byte[10];
        var sut = EncodeableText.Create( Encoding.Unicode, "foo" ).Value;

        var result = sut.Encode( target );

        Assertion.All(
                result.Exception.TestNull(),
                target.TestSequence<byte>( [ 102, 0, 111, 0, 111, 0, 0, 0, 0, 0 ] ) )
            .Go();
    }

    [Fact]
    public void Encode_ShouldReturnInvalidResult_WhenEncoderFailsToEncode()
    {
        var target = new byte[4];
        var sut = EncodeableText.Create( Encoding.Unicode, "foo" ).Value;

        var result = sut.Encode( target );

        result.Exception.TestType().Exact<ArgumentException>().Go();
    }

    private sealed class InvalidEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotSupportedException();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            throw new NotSupportedException();
        }

        public override int GetMaxByteCount(int charCount)
        {
            throw new NotSupportedException();
        }

        public override int GetMaxCharCount(int byteCount)
        {
            throw new NotSupportedException();
        }
    }
}
