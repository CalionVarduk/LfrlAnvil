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

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Any ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( object ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForInteger()
    {
        var result = _sut.GetByDataType( SqliteDataType.Integer );

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Integer ),
                result.DefaultValue.GetValue().TestEquals( 0L ),
                result.RuntimeType.TestEquals( typeof( long ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForReal()
    {
        var result = _sut.GetByDataType( SqliteDataType.Real );

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Real ),
                result.DefaultValue.GetValue().TestEquals( 0.0 ),
                result.RuntimeType.TestEquals( typeof( double ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForText()
    {
        var result = _sut.GetByDataType( SqliteDataType.Text );

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Text ),
                result.DefaultValue.GetValue().TestRefEquals( string.Empty ),
                result.RuntimeType.TestEquals( typeof( string ) ) )
            .Go();
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBlob()
    {
        var result = _sut.GetByDataType( SqliteDataType.Blob );

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Blob ),
                result.DefaultValue.GetValue().TestRefEquals( Array.Empty<byte>() ),
                result.RuntimeType.TestEquals( typeof( byte[] ) ) )
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
                        (SqliteDataType.Any, typeof( object ))
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = _sut.GetDataTypeDefinitions();

        Assertion.All(
                result.Count.TestEquals( 5 ),
                result.Select( t => t.DataType )
                    .TestSetEqual(
                    [
                        SqliteDataType.Any,
                        SqliteDataType.Integer,
                        SqliteDataType.Real,
                        SqliteDataType.Text,
                        SqliteDataType.Blob
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Integer ),
                result.DefaultValue.Value.TestEquals( EnumWithDefault.B ),
                result.RuntimeType.TestEquals( typeof( EnumWithDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithoutDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Integer ),
                result.DefaultValue.Value.TestEquals( EnumWithoutDefault.A ),
                result.RuntimeType.TestEquals( typeof( EnumWithoutDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutAnyValues()
    {
        var result = _sut.GetByType<EmptyEnum>();

        Assertion.All(
                result.DataType.TestRefEquals( SqliteDataType.Integer ),
                result.DefaultValue.Value.TestEquals( default( EmptyEnum ) ),
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
