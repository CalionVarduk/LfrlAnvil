using System.Linq;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDatabaseFactoryOptionsTests : TestsBase
{
    [Fact]
    public void BaseTypeDefinitionsCreator_ShouldReturnDefaultTypeDefinitions()
    {
        var result = MySqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator( "", new MySqlDataTypeProvider() );
        Assertion.All(
                result.GetTypeDefinitions().Count.TestEquals( 22 ),
                result.GetTypeDefinitions()
                    .Select( t => (t.DataType, t.RuntimeType) )
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
                    ] ),
                result.GetDataTypeDefinitions().Count.TestEquals( 21 ),
                result.GetDataTypeDefinitions()
                    .Select( t => t.DataType )
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
    public void BaseNodeInterpretersCreator_ShouldReturnDefaultNodeInterpreters()
    {
        var typeDefinitions = new MySqlColumnTypeDefinitionProviderBuilder().Build();
        var result = MySqlDatabaseFactoryOptions.BaseNodeInterpretersCreator( "", "foo", new MySqlDataTypeProvider(), typeDefinitions );
        result.Options.TestEquals( MySqlNodeInterpreterOptions.Default.SetCommonSchemaName( "foo" ).SetTypeDefinitions( typeDefinitions ) )
            .Go();
    }

    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;

        Assertion.All(
                sut.IndexFilterResolution.TestEquals( SqlOptionalFunctionalityResolution.Ignore ),
                sut.CharacterSetName.TestNull(),
                sut.CollationName.TestNull(),
                sut.IsEncryptionEnabled.TestNull(),
                sut.DefaultNamesCreator.TestRefEquals( SqlHelpers.DefaultNamesCreator ),
                sut.TypeDefinitionsCreator.TestRefEquals( MySqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator ),
                sut.NodeInterpretersCreator.TestRefEquals( MySqlDatabaseFactoryOptions.BaseNodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlOptionalFunctionalityResolution.Ignore )]
    [InlineData( SqlOptionalFunctionalityResolution.Include )]
    [InlineData( SqlOptionalFunctionalityResolution.Forbid )]
    public void SetIndexFilterResolution_ShouldReturnCorrectResult(SqlOptionalFunctionalityResolution resolution)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetIndexFilterResolution( resolution );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( resolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "foo" )]
    public void SetCharacterSetName_ShouldReturnCorrectResult(string? name)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetCharacterSetName( name );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( name ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "foo" )]
    public void SetCollationName_ShouldReturnCorrectResult(string? name)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetCollationName( name );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( name ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableEncryption_ShouldReturnCorrectResult(bool? enabled)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.EnableEncryption( enabled );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( enabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNamesCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( creator );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( creator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNamesCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( null );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( SqlHelpers.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitionsCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>>();
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( creator );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( creator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitionsCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( null );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( MySqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetNodeInterpretersCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider,
                MySqlNodeInterpreterFactory>>();

        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( creator );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( creator ) )
            .Go();
    }

    [Fact]
    public void SetNodeInterpretersCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( null );

        Assertion.All(
                result.IndexFilterResolution.TestEquals( sut.IndexFilterResolution ),
                result.CharacterSetName.TestEquals( sut.CharacterSetName ),
                result.CollationName.TestEquals( sut.CollationName ),
                result.IsEncryptionEnabled.TestEquals( sut.IsEncryptionEnabled ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( MySqlDatabaseFactoryOptions.BaseNodeInterpretersCreator ) )
            .Go();
    }
}
