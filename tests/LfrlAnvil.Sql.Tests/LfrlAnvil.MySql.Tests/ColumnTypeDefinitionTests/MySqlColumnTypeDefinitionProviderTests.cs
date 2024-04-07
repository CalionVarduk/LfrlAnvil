using System.Data;
using System.Linq;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.ColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly MySqlColumnTypeDefinitionProvider _sut =
        new MySqlColumnTypeDefinitionProvider( new MySqlColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void GetByDataType_ShouldReturnBoolForBool()
    {
        var result = _sut.GetByDataType( MySqlDataType.Bool );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Bool );
            result.DefaultValue.GetValue().Should().Be( false );
            result.RuntimeType.Should().Be( typeof( bool ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt8ForTinyInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.TinyInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.TinyInt );
            result.DefaultValue.GetValue().Should().Be( ( sbyte )0 );
            result.RuntimeType.Should().Be( typeof( sbyte ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt8ForUnsignedTinyInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedTinyInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedTinyInt );
            result.DefaultValue.GetValue().Should().Be( ( byte )0 );
            result.RuntimeType.Should().Be( typeof( byte ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt16ForSmallInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.SmallInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.SmallInt );
            result.DefaultValue.GetValue().Should().Be( ( short )0 );
            result.RuntimeType.Should().Be( typeof( short ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt16ForUnsignedSmallInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedSmallInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedSmallInt );
            result.DefaultValue.GetValue().Should().Be( ( ushort )0 );
            result.RuntimeType.Should().Be( typeof( ushort ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt32ForInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.Int );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Int );
            result.DefaultValue.GetValue().Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt32ForUnsignedInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedInt );
            result.DefaultValue.GetValue().Should().Be( ( uint )0 );
            result.RuntimeType.Should().Be( typeof( uint ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForBigInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.BigInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.BigInt );
            result.DefaultValue.GetValue().Should().Be( 0L );
            result.RuntimeType.Should().Be( typeof( long ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt64ForUnsignedBigInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedBigInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedBigInt );
            result.DefaultValue.GetValue().Should().Be( 0UL );
            result.RuntimeType.Should().Be( typeof( ulong ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnFloatForFloat()
    {
        var result = _sut.GetByDataType( MySqlDataType.Float );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Float );
            result.DefaultValue.GetValue().Should().Be( ( float )0.0 );
            result.RuntimeType.Should().Be( typeof( float ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForDouble()
    {
        var result = _sut.GetByDataType( MySqlDataType.Double );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Double );
            result.DefaultValue.GetValue().Should().Be( 0.0 );
            result.RuntimeType.Should().Be( typeof( double ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDecimalForDecimal()
    {
        var result = _sut.GetByDataType( MySqlDataType.Decimal );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Decimal );
            result.DefaultValue.GetValue().Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForChar()
    {
        var result = _sut.GetByDataType( MySqlDataType.Char );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Char );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBinary()
    {
        var result = _sut.GetByDataType( MySqlDataType.Binary );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Binary );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForVarChar()
    {
        var result = _sut.GetByDataType( MySqlDataType.VarChar );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.VarChar );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForVarBinary()
    {
        var result = _sut.GetByDataType( MySqlDataType.VarBinary );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.VarBinary );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBlob()
    {
        var result = _sut.GetByDataType( MySqlDataType.Blob );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Blob );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForText()
    {
        var result = _sut.GetByDataType( MySqlDataType.Text );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Text );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateOnlyForDate()
    {
        var result = _sut.GetByDataType( MySqlDataType.Date );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Date );
            result.DefaultValue.GetValue().Should().Be( DateOnly.FromDateTime( DateTime.UnixEpoch ) );
            result.RuntimeType.Should().Be( typeof( DateOnly ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnTimeOnlyForTime()
    {
        var result = _sut.GetByDataType( MySqlDataType.Time );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Time );
            result.DefaultValue.GetValue().Should().Be( TimeOnly.MinValue );
            result.RuntimeType.Should().Be( typeof( TimeOnly ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForDateTime()
    {
        var result = _sut.GetByDataType( MySqlDataType.DateTime );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.DateTime );
            result.DefaultValue.GetValue().Should().Be( DateTime.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTime ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnObjectForUnknown()
    {
        var result = _sut.GetByDataType( MySqlDataType.Custom( "NULL", MySqlDbType.Null, DbType.Object ) );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Blob );
            result.DefaultValue.GetValue().Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( object ) );
        }
    }

    [Theory]
    [InlineData( MySqlDbType.Decimal )]
    [InlineData( MySqlDbType.NewDecimal )]
    public void GetByDataType_ShouldReturnCustomDecimalForCustomDecimal(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "DECIMAL(10, 0)", dbType, DbType.Decimal );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( type );
            result.DefaultValue.GetValue().Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
            secondResult.Should().BeSameAs( result );
        }
    }

    [Theory]
    [InlineData( MySqlDbType.String )]
    [InlineData( MySqlDbType.VarChar )]
    [InlineData( MySqlDbType.VarString )]
    [InlineData( MySqlDbType.TinyText )]
    [InlineData( MySqlDbType.Text )]
    [InlineData( MySqlDbType.MediumText )]
    [InlineData( MySqlDbType.LongText )]
    public void GetByDataType_ShouldReturnCustomStringForCustomText(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "STRING", dbType, DbType.String );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( type );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
            secondResult.Should().BeSameAs( result );
        }
    }

    [Theory]
    [InlineData( MySqlDbType.Binary )]
    [InlineData( MySqlDbType.VarBinary )]
    [InlineData( MySqlDbType.TinyBlob )]
    [InlineData( MySqlDbType.Blob )]
    [InlineData( MySqlDbType.MediumBlob )]
    [InlineData( MySqlDbType.LongBlob )]
    public void GetByDataType_ShouldReturnCustomByteArrayForCustomBlob(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "BYTEARRAY", dbType, DbType.Binary );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( type );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
            secondResult.Should().BeSameAs( result );
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
                    (MySqlDataType.Bool, typeof( bool )),
                    (MySqlDataType.TinyInt, typeof( sbyte )),
                    (MySqlDataType.UnsignedTinyInt, typeof( byte )),
                    (MySqlDataType.SmallInt, typeof( short )),
                    (MySqlDataType.UnsignedSmallInt, typeof( ushort )),
                    (MySqlDataType.Int, typeof( int )),
                    (MySqlDataType.UnsignedInt, typeof( uint )),
                    (MySqlDataType.BigInt, typeof( long )),
                    (MySqlDataType.UnsignedBigInt, typeof( ulong )),
                    (MySqlDataType.Float, typeof( float )),
                    (MySqlDataType.Double, typeof( double )),
                    (MySqlDataType.Decimal, typeof( decimal )),
                    (MySqlDataType.Text, typeof( string )),
                    (MySqlDataType.Blob, typeof( byte[] )),
                    (MySqlDataType.Date, typeof( DateOnly )),
                    (MySqlDataType.Time, typeof( TimeOnly )),
                    (MySqlDataType.DateTime, typeof( DateTime )),
                    (MySqlDataType.BigInt, typeof( TimeSpan )),
                    (MySqlDataType.CreateChar( 33 ), typeof( DateTimeOffset )),
                    (MySqlDataType.CreateChar( 1 ), typeof( char )),
                    (MySqlDataType.CreateBinary( 16 ), typeof( Guid )),
                    (MySqlDataType.Blob, typeof( object )) );
        }
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = _sut.GetDataTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 21 );
            result.Select( t => t.DataType )
                .Should()
                .BeEquivalentTo(
                    MySqlDataType.Bool,
                    MySqlDataType.TinyInt,
                    MySqlDataType.UnsignedTinyInt,
                    MySqlDataType.SmallInt,
                    MySqlDataType.UnsignedSmallInt,
                    MySqlDataType.Int,
                    MySqlDataType.UnsignedInt,
                    MySqlDataType.BigInt,
                    MySqlDataType.UnsignedBigInt,
                    MySqlDataType.Float,
                    MySqlDataType.Double,
                    MySqlDataType.Decimal,
                    MySqlDataType.Text,
                    MySqlDataType.Char,
                    MySqlDataType.VarChar,
                    MySqlDataType.Blob,
                    MySqlDataType.Binary,
                    MySqlDataType.VarBinary,
                    MySqlDataType.Date,
                    MySqlDataType.Time,
                    MySqlDataType.DateTime );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.BigInt );
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
            result.DataType.Should().BeSameAs( MySqlDataType.Int );
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
            result.DataType.Should().BeSameAs( MySqlDataType.Int );
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
