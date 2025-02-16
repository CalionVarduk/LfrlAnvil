using System.Linq;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDatabaseFactoryOptionsTests : TestsBase
{
    [Fact]
    public void BaseTypeDefinitionsCreator_ShouldReturnDefaultTypeDefinitions()
    {
        var result = PostgreSqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator( "", new PostgreSqlDataTypeProvider() );
        Assertion.All(
                result.GetTypeDefinitions().Count.TestEquals( 22 ),
                result.GetTypeDefinitions()
                    .Select( t => (t.DataType, t.RuntimeType) )
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
                    ] ),
                result.GetDataTypeDefinitions().Count.TestEquals( 14 ),
                result.GetDataTypeDefinitions()
                    .Select( t => t.DataType )
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
    public void BaseNodeInterpretersCreator_ShouldReturnDefaultNodeInterpreters()
    {
        var typeDefinitions = new PostgreSqlColumnTypeDefinitionProviderBuilder().Build();
        var result = PostgreSqlDatabaseFactoryOptions.BaseNodeInterpretersCreator(
            "",
            "foo",
            new PostgreSqlDataTypeProvider(),
            typeDefinitions );

        result.Options.TestEquals( PostgreSqlNodeInterpreterOptions.Default.SetTypeDefinitions( typeDefinitions ) ).Go();
    }

    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;

        Assertion.All(
                sut.VirtualGeneratedColumnStorageResolution.TestEquals( SqlOptionalFunctionalityResolution.Ignore ),
                sut.EncodingName.TestNull(),
                sut.LocaleName.TestNull(),
                sut.ConcurrentConnectionsLimit.TestNull(),
                sut.DefaultNamesCreator.TestRefEquals( SqlHelpers.DefaultNamesCreator ),
                sut.TypeDefinitionsCreator.TestRefEquals( PostgreSqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator ),
                sut.NodeInterpretersCreator.TestRefEquals( PostgreSqlDatabaseFactoryOptions.BaseNodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlOptionalFunctionalityResolution.Ignore )]
    [InlineData( SqlOptionalFunctionalityResolution.Include )]
    [InlineData( SqlOptionalFunctionalityResolution.Forbid )]
    public void SetVirtualGeneratedColumnStorageResolution_ShouldReturnCorrectResult(SqlOptionalFunctionalityResolution resolution)
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetVirtualGeneratedColumnStorageResolution( resolution );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( resolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "foo" )]
    public void SetEncodingName_ShouldReturnCorrectResult(string? name)
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetEncodingName( name );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( name ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "foo" )]
    public void SetLocaleName_ShouldReturnCorrectResult(string? name)
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetLocaleName( name );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( name ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void SetConcurrentConnectionsLimit_ShouldReturnCorrectResult(int? value)
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetConcurrentConnectionsLimit( value );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( value ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNamesCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( creator );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( creator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNamesCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( null );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( SqlHelpers.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitionsCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>>();

        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( creator );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( creator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitionsCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( null );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( PostgreSqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetNodeInterpretersCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
                PostgreSqlNodeInterpreterFactory>>();

        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( creator );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( creator ) )
            .Go();
    }

    [Fact]
    public void SetNodeInterpretersCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( null );

        Assertion.All(
                result.VirtualGeneratedColumnStorageResolution.TestEquals( sut.VirtualGeneratedColumnStorageResolution ),
                result.EncodingName.TestEquals( sut.EncodingName ),
                result.LocaleName.TestEquals( sut.LocaleName ),
                result.ConcurrentConnectionsLimit.TestEquals( sut.ConcurrentConnectionsLimit ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( PostgreSqlDatabaseFactoryOptions.BaseNodeInterpretersCreator ) )
            .Go();
    }
}
