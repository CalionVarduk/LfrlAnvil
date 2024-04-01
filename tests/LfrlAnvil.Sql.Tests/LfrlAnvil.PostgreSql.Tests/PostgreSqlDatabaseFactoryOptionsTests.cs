﻿using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDatabaseFactoryOptionsTests : TestsBase
{
    [Fact]
    public void BaseTypeDefinitionsCreator_ShouldReturnDefaultTypeDefinitions()
    {
        var result = PostgreSqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator( "", new PostgreSqlDataTypeProvider() );
        result.Should().BeEquivalentTo( new PostgreSqlColumnTypeDefinitionProviderBuilder().Build() );
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

        result.Options.Should().BeEquivalentTo( PostgreSqlNodeInterpreterOptions.Default.SetTypeDefinitions( typeDefinitions ) );
    }

    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;

        using ( new AssertionScope() )
        {
            sut.VirtualGeneratedColumnStorageResolution.Should().Be( SqlOptionalFunctionalityResolution.Ignore );
            sut.DefaultNamesCreator.Should().BeSameAs( SqlHelpers.DefaultNamesCreator );
            sut.TypeDefinitionsCreator.Should().BeSameAs( PostgreSqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator );
            sut.NodeInterpretersCreator.Should().BeSameAs( PostgreSqlDatabaseFactoryOptions.BaseNodeInterpretersCreator );
        }
    }

    [Theory]
    [InlineData( SqlOptionalFunctionalityResolution.Ignore )]
    [InlineData( SqlOptionalFunctionalityResolution.Include )]
    [InlineData( SqlOptionalFunctionalityResolution.Forbid )]
    public void SetVirtualGeneratedColumnStorageResolution_ShouldReturnCorrectResult(SqlOptionalFunctionalityResolution resolution)
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetVirtualGeneratedColumnStorageResolution( resolution );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( resolution );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetDefaultNamesCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( creator );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( sut.VirtualGeneratedColumnStorageResolution );
            result.DefaultNamesCreator.Should().BeSameAs( creator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetDefaultNamesCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetDefaultNamesCreator( null );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( sut.VirtualGeneratedColumnStorageResolution );
            result.DefaultNamesCreator.Should().BeSameAs( SqlHelpers.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetTypeDefinitionsCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>>();

        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( creator );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( sut.VirtualGeneratedColumnStorageResolution );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( creator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetTypeDefinitionsCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetTypeDefinitionsCreator( null );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( sut.VirtualGeneratedColumnStorageResolution );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( PostgreSqlDatabaseFactoryOptions.BaseTypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( sut.NodeInterpretersCreator );
        }
    }

    [Fact]
    public void SetNodeInterpretersCreator_ShouldReturnCorrectResult()
    {
        var creator = Substitute
            .For<SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
                PostgreSqlNodeInterpreterFactory>>();

        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( creator );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( sut.VirtualGeneratedColumnStorageResolution );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( creator );
        }
    }

    [Fact]
    public void SetNodeInterpretersCreator_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDatabaseFactoryOptions.Default;
        var result = sut.SetNodeInterpretersCreator( null );

        using ( new AssertionScope() )
        {
            result.VirtualGeneratedColumnStorageResolution.Should().Be( sut.VirtualGeneratedColumnStorageResolution );
            result.DefaultNamesCreator.Should().BeSameAs( sut.DefaultNamesCreator );
            result.TypeDefinitionsCreator.Should().BeSameAs( sut.TypeDefinitionsCreator );
            result.NodeInterpretersCreator.Should().BeSameAs( PostgreSqlDatabaseFactoryOptions.BaseNodeInterpretersCreator );
        }
    }
}
