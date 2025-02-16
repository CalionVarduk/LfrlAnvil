using System.Linq;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDatabaseFactoryOptionsTests : TestsBase
{
    [Fact]
    public void BaseTypeDefinitionsCreator_ShouldReturnDefaultTypeDefinitions()
    {
        var result = SqliteDatabaseFactoryOptions.BaseTypeDefinitionsCreator( "", new SqliteDataTypeProvider() );
        Assertion.All(
                result.GetTypeDefinitions().Count.TestEquals( 22 ),
                result.GetTypeDefinitions()
                    .Select( t => (t.DataType, t.RuntimeType) )
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
                    ] ),
                result.GetDataTypeDefinitions().Count.TestEquals( 5 ),
                result.GetDataTypeDefinitions()
                    .Select( t => t.DataType )
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
    public void BaseNodeInterpretersCreator_ShouldReturnDefaultNodeInterpreters()
    {
        var typeDefinitions = new SqliteColumnTypeDefinitionProviderBuilder().Build();
        var result = SqliteDatabaseFactoryOptions.BaseNodeInterpretersCreator( "", "", new SqliteDataTypeProvider(), typeDefinitions );
        result.Options.TestEquals( SqliteNodeInterpreterOptions.Default.SetTypeDefinitions( typeDefinitions ) ).Go();
    }

    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;

        Assertion.All(
                sut.IsConnectionPermanent.TestFalse(),
                sut.AreForeignKeyChecksDisabled.TestFalse(),
                sut.Encoding.TestNull(),
                sut.DefaultNamesCreator.TestRefEquals( SqlHelpers.DefaultNamesCreator ),
                sut.TypeDefinitionsCreator.TestRefEquals( SqliteDatabaseFactoryOptions.BaseTypeDefinitionsCreator ),
                sut.NodeInterpretersCreator.TestRefEquals( SqliteDatabaseFactoryOptions.BaseNodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableConnectionPermanence_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.EnableConnectionPermanence( enabled );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( enabled ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableForeignKeyChecks_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.EnableForeignKeyChecks( enabled );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( ! enabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( SqliteDatabaseEncoding.UTF_8 )]
    [InlineData( SqliteDatabaseEncoding.UTF_16 )]
    [InlineData( SqliteDatabaseEncoding.UTF_16_LE )]
    [InlineData( SqliteDatabaseEncoding.UTF_16_BE )]
    public void SetEncoding_ShouldReturnCorrectResult(SqliteDatabaseEncoding? value)
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetEncoding( value );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( value ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNamesCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( creator );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( creator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetDefaultNamesCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( null );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( SqlHelpers.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitionsCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>>();
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( creator );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( creator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitionsCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( null );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( SqliteDatabaseFactoryOptions.BaseTypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( sut.NodeInterpretersCreator ) )
            .Go();
    }

    [Fact]
    public void SetNodeInterpretersCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider,
                SqliteNodeInterpreterFactory>>();

        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( creator );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( creator ) )
            .Go();
    }

    [Fact]
    public void SetNodeInterpretersCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( null );

        Assertion.All(
                result.IsConnectionPermanent.TestEquals( sut.IsConnectionPermanent ),
                result.AreForeignKeyChecksDisabled.TestEquals( sut.AreForeignKeyChecksDisabled ),
                result.Encoding.TestEquals( sut.Encoding ),
                result.DefaultNamesCreator.TestRefEquals( sut.DefaultNamesCreator ),
                result.TypeDefinitionsCreator.TestRefEquals( sut.TypeDefinitionsCreator ),
                result.NodeInterpretersCreator.TestRefEquals( SqliteDatabaseFactoryOptions.BaseNodeInterpretersCreator ) )
            .Go();
    }
}
