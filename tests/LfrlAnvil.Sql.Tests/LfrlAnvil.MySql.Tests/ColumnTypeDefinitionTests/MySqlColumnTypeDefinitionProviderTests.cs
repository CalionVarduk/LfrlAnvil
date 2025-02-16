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

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Bool ),
                result.DefaultValue.GetValue().TestEquals( false ),
                result.RuntimeType.TestEquals( typeof( bool ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt8ForTinyInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.TinyInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.TinyInt ),
                result.DefaultValue.GetValue().TestEquals( ( sbyte )0 ),
                result.RuntimeType.TestEquals( typeof( sbyte ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt8ForUnsignedTinyInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedTinyInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.UnsignedTinyInt ),
                result.DefaultValue.GetValue().TestEquals( ( byte )0 ),
                result.RuntimeType.TestEquals( typeof( byte ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt16ForSmallInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.SmallInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.SmallInt ),
                result.DefaultValue.GetValue().TestEquals( ( short )0 ),
                result.RuntimeType.TestEquals( typeof( short ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt16ForUnsignedSmallInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedSmallInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.UnsignedSmallInt ),
                result.DefaultValue.GetValue().TestEquals( ( ushort )0 ),
                result.RuntimeType.TestEquals( typeof( ushort ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt32ForInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.Int );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Int ),
                result.DefaultValue.GetValue().TestEquals( 0 ),
                result.RuntimeType.TestEquals( typeof( int ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt32ForUnsignedInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.UnsignedInt ),
                result.DefaultValue.GetValue().TestEquals( ( uint )0 ),
                result.RuntimeType.TestEquals( typeof( uint ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForBigInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.BigInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.BigInt ),
                result.DefaultValue.GetValue().TestEquals( 0L ),
                result.RuntimeType.TestEquals( typeof( long ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt64ForUnsignedBigInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedBigInt );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.UnsignedBigInt ),
                result.DefaultValue.GetValue().TestEquals( 0UL ),
                result.RuntimeType.TestEquals( typeof( ulong ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnFloatForFloat()
    {
        var result = _sut.GetByDataType( MySqlDataType.Float );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Float ),
                result.DefaultValue.GetValue().TestEquals( ( float )0.0 ),
                result.RuntimeType.TestEquals( typeof( float ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForDouble()
    {
        var result = _sut.GetByDataType( MySqlDataType.Double );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Double ),
                result.DefaultValue.GetValue().TestEquals( 0.0 ),
                result.RuntimeType.TestEquals( typeof( double ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDecimalForDecimal()
    {
        var result = _sut.GetByDataType( MySqlDataType.Decimal );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Decimal ),
                result.DefaultValue.GetValue().TestEquals( 0m ),
                result.RuntimeType.TestEquals( typeof( decimal ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForChar()
    {
        var result = _sut.GetByDataType( MySqlDataType.Char );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Char ),
                result.DefaultValue.GetValue().TestEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBinary()
    {
        var result = _sut.GetByDataType( MySqlDataType.Binary );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Binary ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( byte[] ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForVarChar()
    {
        var result = _sut.GetByDataType( MySqlDataType.VarChar );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.VarChar ),
                result.DefaultValue.GetValue().TestEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForVarBinary()
    {
        var result = _sut.GetByDataType( MySqlDataType.VarBinary );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.VarBinary ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( byte[] ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBlob()
    {
        var result = _sut.GetByDataType( MySqlDataType.Blob );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Blob ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( byte[] ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForText()
    {
        var result = _sut.GetByDataType( MySqlDataType.Text );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Text ),
                result.DefaultValue.GetValue().TestEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateOnlyForDate()
    {
        var result = _sut.GetByDataType( MySqlDataType.Date );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Date ),
                result.DefaultValue.GetValue().TestEquals( DateOnly.FromDateTime( DateTime.UnixEpoch ) ),
                result.RuntimeType.TestEquals( typeof( DateOnly ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnTimeOnlyForTime()
    {
        var result = _sut.GetByDataType( MySqlDataType.Time );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Time ),
                result.DefaultValue.GetValue().TestEquals( TimeOnly.MinValue ),
                result.RuntimeType.TestEquals( typeof( TimeOnly ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForDateTime()
    {
        var result = _sut.GetByDataType( MySqlDataType.DateTime );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.DateTime ),
                result.DefaultValue.GetValue().TestEquals( DateTime.UnixEpoch ),
                result.RuntimeType.TestEquals( typeof( DateTime ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnObjectForUnknown()
    {
        var result = _sut.GetByDataType( MySqlDataType.Custom( "NULL", MySqlDbType.Null, DbType.Object ) );

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Blob ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( object ) ) )
            .Go();
    }

    [Theory]
    [InlineData( MySqlDbType.Decimal )]
    [InlineData( MySqlDbType.NewDecimal )]
    public void GetByDataType_ShouldReturnCustomDecimalForCustomDecimal(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "DECIMAL(10, 0)", dbType, DbType.Decimal );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        Assertion.All(
                result.DataType.TestRefEquals( type ),
                result.DefaultValue.GetValue().TestEquals( 0m ),
                result.RuntimeType.TestEquals( typeof( decimal ) ),
                secondResult.TestRefEquals( result ) )
            .Go();
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

        Assertion.All(
                result.DataType.TestRefEquals( type ),
                result.DefaultValue.GetValue().TestEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ),
                secondResult.TestRefEquals( result ) )
            .Go();
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

        Assertion.All(
                result.DataType.TestRefEquals( type ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( byte[] ) ),
                secondResult.TestRefEquals( result ) )
            .Go();
    }

    [Fact]
    public void GetTypeDefinitions_ShouldReturnAllTypeDefinitionsRegisteredByRuntimeType()
    {
        var result = _sut.GetTypeDefinitions();

        Assertion.All(
                result.Count.TestEquals( 22 ),
                result.Select( t => (t.DataType, t.RuntimeType) )
                    .TestSetEqual(
                    [
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
                        (MySqlDataType.Blob, typeof( object ))
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = _sut.GetDataTypeDefinitions();

        Assertion.All(
                result.Count.TestEquals( 21 ),
                result.Select( t => t.DataType )
                    .TestSetEqual(
                    [
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
                        MySqlDataType.DateTime
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.BigInt ),
                result.DefaultValue.Value.TestEquals( EnumWithDefault.B ),
                result.RuntimeType.TestEquals( typeof( EnumWithDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithoutDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Int ),
                result.DefaultValue.Value.TestEquals( EnumWithoutDefault.A ),
                result.RuntimeType.TestEquals( typeof( EnumWithoutDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutAnyValues()
    {
        var result = _sut.GetByType<EmptyEnum>();

        Assertion.All(
                result.DataType.TestRefEquals( MySqlDataType.Int ),
                result.DefaultValue.Value.TestEquals( default ),
                result.RuntimeType.TestEquals( typeof( EmptyEnum ) ) )
            .Go();
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
