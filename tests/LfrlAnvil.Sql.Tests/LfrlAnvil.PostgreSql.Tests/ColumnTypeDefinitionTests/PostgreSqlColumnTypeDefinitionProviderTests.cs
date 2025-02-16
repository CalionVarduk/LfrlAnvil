using System.Data;
using System.Linq;
using LfrlAnvil.Sql.Extensions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _sut =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void GetByDataType_ShouldReturnBoolForBoolean()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Boolean );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Boolean ),
                result.DefaultValue.GetValue().TestEquals( false ),
                result.RuntimeType.TestEquals( typeof( bool ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt16ForInt2()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Int2 );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Int2 ),
                result.DefaultValue.GetValue().TestEquals( ( short )0 ),
                result.RuntimeType.TestEquals( typeof( short ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt32ForInt4()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Int4 );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Int4 ),
                result.DefaultValue.GetValue().TestEquals( 0 ),
                result.RuntimeType.TestEquals( typeof( int ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForInt8()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Int8 );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Int8 ),
                result.DefaultValue.GetValue().TestEquals( ( long )0 ),
                result.RuntimeType.TestEquals( typeof( long ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnFloatForFloat4()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Float4 );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Float4 ),
                result.DefaultValue.GetValue().TestEquals( 0.0F ),
                result.RuntimeType.TestEquals( typeof( float ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForFloat8()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Float8 );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Float8 ),
                result.DefaultValue.GetValue().TestEquals( 0.0 ),
                result.RuntimeType.TestEquals( typeof( double ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDecimalForDecimal()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Decimal );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Decimal ),
                result.DefaultValue.GetValue().TestEquals( 0m ),
                result.RuntimeType.TestEquals( typeof( decimal ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForVarChar()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.VarChar );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.VarChar ),
                result.DefaultValue.GetValue().TestEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBytea()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Bytea );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Bytea ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( byte[] ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnGuidForUuid()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Uuid );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Uuid ),
                result.DefaultValue.GetValue().TestEquals( Guid.Empty ),
                result.RuntimeType.TestEquals( typeof( Guid ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateOnlyForDate()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Date );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Date ),
                result.DefaultValue.GetValue().TestEquals( DateOnly.FromDateTime( DateTime.UnixEpoch ) ),
                result.RuntimeType.TestEquals( typeof( DateOnly ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnTimeOnlyForTime()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Time );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Time ),
                result.DefaultValue.GetValue().TestEquals( TimeOnly.MinValue ),
                result.RuntimeType.TestEquals( typeof( TimeOnly ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForTimestamp()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Timestamp );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Timestamp ),
                result.DefaultValue.GetValue().TestEquals( DateTime.UnixEpoch ),
                result.RuntimeType.TestEquals( typeof( DateTime ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForTimestampTz()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.TimestampTz );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.TimestampTz ),
                result.DefaultValue.GetValue().TestEquals( DateTime.UnixEpoch ),
                result.RuntimeType.TestEquals( typeof( DateTime ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnObjectForUnknown()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Custom( "NULL", NpgsqlDbType.Unknown, DbType.Object ) );

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Bytea ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( object ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnCustomDecimalForCustomNumeric()
    {
        var type = PostgreSqlDataType.Custom( "DECIMAL(10, 0)", NpgsqlDbType.Numeric, DbType.Decimal );
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
    [InlineData( NpgsqlDbType.Text )]
    [InlineData( NpgsqlDbType.Char )]
    [InlineData( NpgsqlDbType.Varchar )]
    public void GetByDataType_ShouldReturnCustomStringForCustomChar(NpgsqlDbType dbType)
    {
        var type = PostgreSqlDataType.Custom( "STRING", dbType, DbType.String );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        Assertion.All(
                result.DataType.TestRefEquals( type ),
                result.DefaultValue.GetValue().TestEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ),
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
                        (PostgreSqlDataType.Boolean, typeof( bool )),
                        (PostgreSqlDataType.Int2, typeof( short )),
                        (PostgreSqlDataType.Int4, typeof( int )),
                        (PostgreSqlDataType.Int8, typeof( long )),
                        (PostgreSqlDataType.Float4, typeof( float )),
                        (PostgreSqlDataType.Float8, typeof( double )),
                        (PostgreSqlDataType.Decimal, typeof( decimal )),
                        (PostgreSqlDataType.VarChar, typeof( string )),
                        (PostgreSqlDataType.Bytea, typeof( byte[] )),
                        (PostgreSqlDataType.Uuid, typeof( Guid )),
                        (PostgreSqlDataType.Date, typeof( DateOnly )),
                        (PostgreSqlDataType.Time, typeof( TimeOnly )),
                        (PostgreSqlDataType.Timestamp, typeof( DateTime )),
                        (PostgreSqlDataType.CreateVarChar( 1 ), typeof( char )),
                        (PostgreSqlDataType.CreateVarChar( 33 ), typeof( DateTimeOffset )),
                        (PostgreSqlDataType.Int2, typeof( sbyte )),
                        (PostgreSqlDataType.Int8, typeof( TimeSpan )),
                        (PostgreSqlDataType.Int2, typeof( byte )),
                        (PostgreSqlDataType.Int4, typeof( ushort )),
                        (PostgreSqlDataType.Int8, typeof( uint )),
                        (PostgreSqlDataType.Int8, typeof( ulong )),
                        (PostgreSqlDataType.Bytea, typeof( object ))
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = _sut.GetDataTypeDefinitions();

        Assertion.All(
                result.Count.TestEquals( 14 ),
                result.Select( t => t.DataType )
                    .TestSetEqual(
                    [
                        PostgreSqlDataType.Boolean,
                        PostgreSqlDataType.Int2,
                        PostgreSqlDataType.Int4,
                        PostgreSqlDataType.Int8,
                        PostgreSqlDataType.Float4,
                        PostgreSqlDataType.Float8,
                        PostgreSqlDataType.Decimal,
                        PostgreSqlDataType.VarChar,
                        PostgreSqlDataType.Bytea,
                        PostgreSqlDataType.Uuid,
                        PostgreSqlDataType.Date,
                        PostgreSqlDataType.Time,
                        PostgreSqlDataType.Timestamp,
                        PostgreSqlDataType.TimestampTz
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Int8 ),
                result.DefaultValue.Value.TestEquals( EnumWithDefault.B ),
                result.RuntimeType.TestEquals( typeof( EnumWithDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithoutDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Int4 ),
                result.DefaultValue.Value.TestEquals( EnumWithoutDefault.A ),
                result.RuntimeType.TestEquals( typeof( EnumWithoutDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutAnyValues()
    {
        var result = _sut.GetByType<EmptyEnum>();

        Assertion.All(
                result.DataType.TestRefEquals( PostgreSqlDataType.Int4 ),
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
