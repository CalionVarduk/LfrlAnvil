using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDatabaseFactoryOptionsTests : TestsBase
{
    [Fact]
    public void BaseTypeDefinitionsCreator_ShouldReturnDefaultTypeDefinitions()
    {
        var result = MySqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator( "", new MySqlDataTypeProvider() );
        result.Should().BeEquivalentTo( new MySqlColumnTypeDefinitionProviderBuilder().Build() );
    }

    [Fact]
    public void BaseNodeInterpretersCreator_ShouldReturnDefaultNodeInterpreters()
    {
        var typeDefinitions = new MySqlColumnTypeDefinitionProviderBuilder().Build();
        var result = MySqlDatabaseFactoryOptions.BaseNodeInterpretersCreator( "", "foo", new MySqlDataTypeProvider(), typeDefinitions );
        result.Options.Should()
            .BeEquivalentTo( MySqlNodeInterpreterOptions.Default.SetCommonSchemaName( "foo" ).SetTypeDefinitions( typeDefinitions ) );
    }

    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;

        using ( new AssertionScope() )
        {
            sut.IndexFilterResolution.Should().Be( SqlOptionalFunctionalityResolution.Ignore );
            sut.CharacterSetName.Should().BeNull();
            sut.CollationName.Should().BeNull();
            sut.IsEncryptionEnabled.Should().BeNull();
            sut.DefaultNamesCreator.Should().BeSameAs( SqlHelpers.DefaultNamesCreator );
            sut.TypeDefinitionsCreator.Should().BeSameAs( MySqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator );
            sut.NodeInterpretersCreator.Should().BeSameAs( MySqlDatabaseFactoryOptions.BaseNodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( SqlOptionalFunctionalityResolution.Ignore )]
    [InlineData( SqlOptionalFunctionalityResolution.Include )]
    [InlineData( SqlOptionalFunctionalityResolution.Forbid )]
    public void SetIndexFilterResolution_ShouldReturnCorrectResult(SqlOptionalFunctionalityResolution resolution)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetIndexFilterResolution( resolution );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( resolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "foo" )]
    public void SetCharacterSetName_ShouldReturnCorrectResult(string? name)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetCharacterSetName( name );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( name );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( "foo" )]
    public void SetCollationName_ShouldReturnCorrectResult(string? name)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetCollationName( name );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( name );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableEncryption_ShouldReturnCorrectResult(bool? enabled)
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.EnableEncryption( enabled );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( enabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetDefaultNamesCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( creator );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( creator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetDefaultNamesCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( null );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( SqlHelpers.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetTypeDefinitionsCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>>();
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( creator );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( creator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetTypeDefinitionsCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( null );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( MySqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetNodeInterpretersCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider,
                MySqlNodeInterpreterFactory>>();

        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( creator );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( creator );
        }
    }

    [Fact]
    public void SetNodeInterpretersCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( null );

        using ( new AssertionScope() )
        {
            result.IndexFilterResolution.Should().Be( sut.IndexFilterResolution );
            result.CharacterSetName.Should().Be( sut.CharacterSetName );
            result.CollationName.Should().Be( sut.CollationName );
            result.IsEncryptionEnabled.Should().Be( sut.IsEncryptionEnabled );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( MySqlDatabaseFactoryOptions.BaseNodeInterpretersCreator );
        }
    }
}
