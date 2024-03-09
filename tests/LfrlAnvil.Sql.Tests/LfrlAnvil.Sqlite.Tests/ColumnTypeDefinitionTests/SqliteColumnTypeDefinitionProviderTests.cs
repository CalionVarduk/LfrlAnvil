using System.Linq;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _sut =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void GetByDataType_ShouldReturnObjectForAny()
    {
        var result = _sut.GetByDataType( SqliteDataType.Any );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Any );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be<object>();
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForInteger()
    {
        var result = _sut.GetByDataType( SqliteDataType.Integer );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.GetValue().Should().Be( 0L );
            result.RuntimeType.Should().Be<long>();
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForReal()
    {
        var result = _sut.GetByDataType( SqliteDataType.Real );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Real );
            result.DefaultValue.GetValue().Should().Be( 0.0 );
            result.RuntimeType.Should().Be<double>();
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForText()
    {
        var result = _sut.GetByDataType( SqliteDataType.Text );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.GetValue().Should().BeSameAs( string.Empty );
            result.RuntimeType.Should().Be<string>();
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBlob()
    {
        var result = _sut.GetByDataType( SqliteDataType.Blob );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Blob );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be<byte[]>();
        }
    }

    [Fact]
    public void GetTypeDefinitions_ShouldReturnAllTypeDefinitionsRegisteredByRuntimeType()
    {
        var result = _sut.GetTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 22 );
            result.Select( t => (t.DataType, t.RuntimeType) )
                .Should()
                .BeEquivalentTo(
                    (SqliteDataType.Integer, typeof( long )),
                    (SqliteDataType.Real, typeof( double )),
                    (SqliteDataType.Text, typeof( string )),
                    (SqliteDataType.Integer, typeof( bool )),
                    (SqliteDataType.Blob, typeof( byte[] )),
                    (SqliteDataType.Integer, typeof( byte )),
                    (SqliteDataType.Integer, typeof( sbyte )),
                    (SqliteDataType.Integer, typeof( ushort )),
                    (SqliteDataType.Integer, typeof( short )),
                    (SqliteDataType.Integer, typeof( uint )),
                    (SqliteDataType.Integer, typeof( int )),
                    (SqliteDataType.Integer, typeof( ulong )),
                    (SqliteDataType.Integer, typeof( TimeSpan )),
                    (SqliteDataType.Real, typeof( float )),
                    (SqliteDataType.Text, typeof( DateTime )),
                    (SqliteDataType.Text, typeof( DateTimeOffset )),
                    (SqliteDataType.Text, typeof( DateOnly )),
                    (SqliteDataType.Text, typeof( TimeOnly )),
                    (SqliteDataType.Text, typeof( decimal )),
                    (SqliteDataType.Text, typeof( char )),
                    (SqliteDataType.Blob, typeof( Guid )),
                    (SqliteDataType.Any, typeof( object )) );
        }
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = _sut.GetDataTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 5 );
            result.Select( t => t.DataType )
                .Should()
                .BeEquivalentTo(
                    SqliteDataType.Any,
                    SqliteDataType.Integer,
                    SqliteDataType.Real,
                    SqliteDataType.Text,
                    SqliteDataType.Blob );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( EnumWithDefault.B );
            result.RuntimeType.Should().Be( typeof( EnumWithDefault ) );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithoutDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( EnumWithoutDefault.A );
            result.RuntimeType.Should().Be( typeof( EnumWithoutDefault ) );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutAnyValues()
    {
        var result = _sut.GetByType<EmptyEnum>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( default( EmptyEnum ) );
            result.RuntimeType.Should().Be( typeof( EmptyEnum ) );
        }
    }

    public enum EmptyEnum { }

    public enum EnumWithoutDefault
    {
        A = 5,
        B = 10,
        C = 20
    }

    public enum EnumWithDefault : long
    {
        A = -1,
        B = 0,
        C = 1
    }
}
