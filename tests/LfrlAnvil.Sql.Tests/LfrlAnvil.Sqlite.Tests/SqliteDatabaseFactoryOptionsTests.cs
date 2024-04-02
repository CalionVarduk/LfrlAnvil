using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDatabaseFactoryOptionsTests : TestsBase
{
    [Fact]
    public void BaseTypeDefinitionsCreator_ShouldReturnDefaultTypeDefinitions()
    {
        var result = SqliteDatabaseFactoryOptions.BaseTypeDefinitionsCreator( "", new SqliteDataTypeProvider() );
        result.Should().BeEquivalentTo( new SqliteColumnTypeDefinitionProviderBuilder().Build() );
    }

    [Fact]
    public void BaseNodeInterpretersCreator_ShouldReturnDefaultNodeInterpreters()
    {
        var typeDefinitions = new SqliteColumnTypeDefinitionProviderBuilder().Build();
        var result = SqliteDatabaseFactoryOptions.BaseNodeInterpretersCreator( "", "", new SqliteDataTypeProvider(), typeDefinitions );
        result.Options.Should().BeEquivalentTo( SqliteNodeInterpreterOptions.Default.SetTypeDefinitions( typeDefinitions ) );
    }

    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;

        using ( new AssertionScope() )
        {
            sut.IsConnectionPermanent.Should().BeFalse();
            sut.AreForeignKeyChecksDisabled.Should().BeFalse();
            sut.Encoding.Should().BeNull();
            sut.DefaultNamesCreator.Should().BeSameAs( SqlHelpers.DefaultNamesCreator );
            sut.TypeDefinitionsCreator.Should().BeSameAs( SqliteDatabaseFactoryOptions.BaseTypeDefinitionsCreator );
            sut.NodeInterpretersCreator.Should().BeSameAs( SqliteDatabaseFactoryOptions.BaseNodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableConnectionPermanence_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.EnableConnectionPermanence( enabled );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( enabled );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableForeignKeyChecks_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.EnableForeignKeyChecks( enabled );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( ! enabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
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

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( value );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetDefaultNamesCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( creator );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( creator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetDefaultNamesCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( null );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( SqlHelpers.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetTypeDefinitionsCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>>();
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( creator );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( creator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetTypeDefinitionsCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( null );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( SqliteDatabaseFactoryOptions.BaseTypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetNodeInterpretersCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider,
                SqliteNodeInterpreterFactory>>();

        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( creator );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( creator );
        }
    }

    [Fact]
    public void SetNodeInterpretersCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( null );

        using ( new AssertionScope() )
        {
            result.IsConnectionPermanent.Should().Be( sut.IsConnectionPermanent );
            result.AreForeignKeyChecksDisabled.Should().Be( sut.AreForeignKeyChecksDisabled );
            result.Encoding.Should().Be( sut.Encoding );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( SqliteDatabaseFactoryOptions.BaseNodeInterpretersCreator );
        }
    }
}
