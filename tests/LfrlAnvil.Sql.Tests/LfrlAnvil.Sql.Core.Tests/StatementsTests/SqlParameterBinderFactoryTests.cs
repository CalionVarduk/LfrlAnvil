using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderFactoryTests : TestsBase
{
    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WhenSourceIsEmpty()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, Enumerable.Empty<SqlParameter>() );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.GetAll().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WhenSourceIsNotEmpty()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", 1 ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithAllowedNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Object );
            command.Parameters[0].IsNullable.Should().BeTrue();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().BeSameAs( DBNull.Value );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithIgnoredNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithPreExistingCommandParameters()
    {
        var command = new DbCommandMock();
        var p1 = command.CreateParameter();
        var p2 = command.CreateParameter();
        command.Parameters.Add( p1 );
        command.Parameters.Add( p2 );

        var parameters = new SqlNamedParameterCollection();
        parameters.TryAdd( "a", 1 );
        parameters.TryAdd( "b", "foo" );
        parameters.TryAdd( "c", 5.0 );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, parameters );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 3 );
            command.Parameters[0].Should().BeSameAs( p1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( 1 );
            command.Parameters[1].Should().BeSameAs( p2 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "b" );
            command.Parameters[1].Value.Should().Be( "foo" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Double );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "c" );
            command.Parameters[2].Value.Should().Be( 5.0 );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithPositionalParameters()
    {
        var command = new DbCommandMock();
        var p1 = command.CreateParameter();
        var p2 = command.CreateParameter();
        command.Parameters.Add( p1 );
        command.Parameters.Add( p2 );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind(
            command,
            new[] { SqlParameter.Positional( 1 ), SqlParameter.Positional( "foo" ), SqlParameter.Positional( 5.0 ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 3 );
            command.Parameters[0].Should().BeSameAs( p1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().BeNull();
            command.Parameters[0].Value.Should().Be( 1 );
            command.Parameters[1].Should().BeSameAs( p2 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().BeNull();
            command.Parameters[1].Value.Should().Be( "foo" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Double );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().BeNull();
            command.Parameters[2].Value.Should().Be( 5.0 );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithAllPreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithSomePreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        var p1 = command.CreateParameter();
        command.Parameters.Add( p1 );
        command.Parameters.Add( command.CreateParameter() );
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", "foo" ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Should().BeSameAs( p1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceIsEmpty()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, Enumerable.Empty<SqlParameter>() );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceIsNotEmpty()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", 1 ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WithAllowedNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Object );
            command.Parameters[0].IsNullable.Should().BeTrue();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().BeSameAs( DBNull.Value );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WithIgnoredNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsStringValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", "foo" ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsByteArrayValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new byte[] { 0, 1, 2 } ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Binary );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().BeEquivalentTo( new byte[] { 0, 1, 2 } );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsEmptyReducibleCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", Array.Empty<int>() ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsNonEmptyReducibleCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new[] { "foo", "bar" } ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a1" );
            command.Parameters[0].Value.Should().Be( "foo" );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "a2" );
            command.Parameters[1].Value.Should().Be( "bar" );
        }
    }

    [Fact]
    public void
        Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsReducibleCollectionWithIgnoredNullElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new[] { null, "foo" } ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a1" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void
        Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsReducibleCollectionWithAllowedNullElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new[] { null, "foo" } ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Object );
            command.Parameters[0].IsNullable.Should().BeTrue();
            command.Parameters[0].ParameterName.Should().Be( "a1" );
            command.Parameters[0].Value.Should().BeSameAs( DBNull.Value );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "a2" );
            command.Parameters[1].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollectionsAndAllPreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollectionsAndSomePreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        var p1 = command.CreateParameter();
        command.Parameters.Add( p1 );
        command.Parameters.Add( command.CreateParameter() );
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", "foo" ) } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Should().BeSameAs( p1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceIsNotProvided()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>();

        parameterBinder.Bind( command );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceIsProvided()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>();

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                C = 5.0,
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 4 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "B" );
            command.Parameters[1].Value.Should().Be( "foo" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Double );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "C" );
            command.Parameters[2].Value.Should().Be( 5.0 );
            command.Parameters[3].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[3].DbType.Should().Be( DbType.Boolean );
            command.Parameters[3].IsNullable.Should().BeFalse();
            command.Parameters[3].ParameterName.Should().Be( "D" );
            command.Parameters[3].Value.Should().Be( true );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceIsProvided_WithPositionalParameters()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .EnableIgnoringOfNullValues( false )
                .With( SqlParameterConfiguration.Positional( "A", 2 ) )
                .With( SqlParameterConfiguration.Positional( "B", 3 ) )
                .With( SqlParameterConfiguration.Positional( "C", 0 ) )
                .With( SqlParameterConfiguration.Positional( "D", 1 ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                C = 5.0,
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 4 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Double );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().BeNull();
            command.Parameters[0].Value.Should().Be( 5.0 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.Boolean );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().BeNull();
            command.Parameters[1].Value.Should().Be( true );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Int32 );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().BeNull();
            command.Parameters[2].Value.Should().Be( 10 );
            command.Parameters[3].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[3].DbType.Should().Be( DbType.String );
            command.Parameters[3].IsNullable.Should().BeFalse();
            command.Parameters[3].ParameterName.Should().BeNull();
            command.Parameters[3].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceIsProvided_WithMixedParameters()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .EnableIgnoringOfNullValues( false )
                .With( SqlParameterConfiguration.Positional( "B", 1 ) )
                .With( SqlParameterConfiguration.Positional( "C", 0 ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                C = 5.0,
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 4 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Double );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().BeNull();
            command.Parameters[0].Value.Should().Be( 5.0 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().BeNull();
            command.Parameters[1].Value.Should().Be( "foo" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Int32 );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "A" );
            command.Parameters[2].Value.Should().Be( 10 );
            command.Parameters[3].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[3].DbType.Should().Be( DbType.Boolean );
            command.Parameters[3].IsNullable.Should().BeFalse();
            command.Parameters[3].ParameterName.Should().Be( "D" );
            command.Parameters[3].Value.Should().Be( true );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceIsProvided_WithUnsupportedPositionalParameters()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance( arePositionalParametersSupported: false );
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .With( SqlParameterConfiguration.Positional( "A", 2 ) )
                .With( SqlParameterConfiguration.Positional( "B", 3 ) )
                .With( SqlParameterConfiguration.Positional( "C", 0 ) )
                .With( SqlParameterConfiguration.Positional( "D", 1 ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                C = 5.0,
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 4 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "B" );
            command.Parameters[1].Value.Should().Be( "foo" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Double );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "C" );
            command.Parameters[2].Value.Should().Be( 5.0 );
            command.Parameters[3].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[3].DbType.Should().Be( DbType.Boolean );
            command.Parameters[3].IsNullable.Should().BeFalse();
            command.Parameters[3].ParameterName.Should().Be( "D" );
            command.Parameters[3].Value.Should().Be( true );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceTypeContainsFieldMembers()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<SourceWithFields>();
        parameterBinder.Bind(
            command,
            new SourceWithFields
            {
                A = 10,
                B = "foo"
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "B" );
            command.Parameters[1].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenMemberIsIgnored()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default.With( SqlParameterConfiguration.IgnoreMember( "A" ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo"
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "B" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceTypeContainsWriteOnlyPropertyMembers()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<SourceWithWriteOnlyProperty>(
            SqlParameterBinderCreationOptions.Default.With( SqlParameterConfiguration.IgnoreMember( "_b" ) ) );

        parameterBinder.Bind(
            command,
            new SourceWithWriteOnlyProperty
            {
                A = 10,
                B = "foo"
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenParameterIsFromOtherSourceMember()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default.With( SqlParameterConfiguration.From( "E", "B" ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo"
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "E" );
            command.Parameters[1].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenCustomSelectorOfRefTypeIsDefined()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .With( SqlParameterConfiguration.From( "E", (Source s) => $"{s.B}{s.D}" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "E" );
            command.Parameters[1].Value.Should().Be( "fooTrue" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenCustomSelectorOfValueTypeIsDefined()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .With( SqlParameterConfiguration.From( "E", (Source s) => s.D == true ? 50 : 100 ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.Int32 );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "E" );
            command.Parameters[1].Value.Should().Be( 50 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenCustomSelectorOverridesSourceMember()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .With( SqlParameterConfiguration.From( "A", (Source s) => $"{s.B}{s.D}" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                D = false
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( "fooFalse" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceTypeIsValidInRelationToProvidedContext()
    {
        var command = new DbCommandMock();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "a", isNullable: true ) );
        interpreter.Visit( SqlNode.Parameter<string>( "b", isNullable: true ) );
        interpreter.Visit( SqlNode.Parameter<double>( "c", isNullable: true ) );
        interpreter.Visit( SqlNode.Parameter<bool>( "d", isNullable: true ) );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ).SetContext( interpreter.Context ) );

        parameterBinder.Bind(
            command,
            new Source
            {
                A = 10,
                B = "foo",
                D = true
            } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 4 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "B" );
            command.Parameters[1].Value.Should().Be( "foo" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Double );
            command.Parameters[2].IsNullable.Should().BeTrue();
            command.Parameters[2].ParameterName.Should().Be( "C" );
            command.Parameters[2].Value.Should().BeSameAs( DBNull.Value );
            command.Parameters[3].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[3].DbType.Should().Be( DbType.Boolean );
            command.Parameters[3].IsNullable.Should().BeFalse();
            command.Parameters[3].ParameterName.Should().Be( "D" );
            command.Parameters[3].Value.Should().Be( true );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenProvidedContextContainsTypelessParameterThatExistsInSourceType()
    {
        var command = new DbCommandMock();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter( "a" ) );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) );

        parameterBinder.Bind( command, new Source { A = 10 } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void
        Create_Generic_ShouldCreateCorrectParameterBinder_WhenProvidedContextContainsPositionalParameterThatExistsInSourceTypeAsNamed()
    {
        var command = new DbCommandMock();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter( "a", index: 0 ) );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int>>(
            SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ).SetContext( interpreter.Context ) );

        parameterBinder.Bind( command, new GenericSource<int>( 10 ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().BeNull();
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void
        Create_Generic_ShouldCreateCorrectParameterBinder_WhenProvidedContextContainsPositionalParameterThatExistsInSourceTypeAsPositional(
            int memberIndex)
    {
        var command = new DbCommandMock();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter( "a", index: 0 ) );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int>>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .EnableIgnoringOfNullValues( false )
                .With( SqlParameterConfiguration.Positional( "A", memberIndex ) ) );

        parameterBinder.Bind( command, new GenericSource<int>( 10 ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().BeNull();
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void
        Create_Generic_ShouldCreateCorrectParameterBinder_WhenProvidedContextContainsNamedParameterThatExistsInSourceTypeAsPositional()
    {
        var command = new DbCommandMock();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter( "a" ) );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int>>(
            SqlParameterBinderCreationOptions.Default.SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.Positional( "A", 1 ) ) );

        parameterBinder.Bind( command, new GenericSource<int>( 10 ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNotNullRefType()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string>>();

        parameterBinder.Bind( command, new GenericSource<string>( "foo" ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNotNullValueType()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int>>();

        parameterBinder.Bind( command, new GenericSource<int>( 10 ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableRefTypeWithValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?>>();

        parameterBinder.Bind( command, new GenericSource<string?>( "foo" ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableRefTypeWithIgnoredNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?>>();

        parameterBinder.Bind( command, new GenericSource<string?>( null ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableRefTypeWithIncludedNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?>>(
            SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<string?>( null ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeTrue();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().BeSameAs( DBNull.Value );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableValueTypeWithValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?>>();

        parameterBinder.Bind( command, new GenericSource<int?>( 10 ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableValueTypeWithIgnoredNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?>>();

        parameterBinder.Bind( command, new GenericSource<int?>( null ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableValueTypeWithIncludedNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?>>(
            SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<int?>( null ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeTrue();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().BeSameAs( DBNull.Value );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithNotNullValueTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int[]>( new[] { 10, 20, 30 } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 3 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A1" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.Int32 );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "A2" );
            command.Parameters[1].Value.Should().Be( 20 );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Int32 );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "A3" );
            command.Parameters[2].Value.Should().Be( 30 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIgnoredNullableValueTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int?[]>( new int?[] { 10, null, 30 } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A1" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.Int32 );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "A2" );
            command.Parameters[1].Value.Should().Be( 30 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIncludedNullableValueTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?[]>>(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<int?[]>( new int?[] { 10, null, 30 } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 3 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A1" );
            command.Parameters[0].Value.Should().Be( 10 );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.Int32 );
            command.Parameters[1].IsNullable.Should().BeTrue();
            command.Parameters[1].ParameterName.Should().Be( "A2" );
            command.Parameters[1].Value.Should().BeSameAs( DBNull.Value );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.Int32 );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "A3" );
            command.Parameters[2].Value.Should().Be( 30 );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithNotNullRefTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<string[]>( new[] { "foo", "bar", "qux" } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 3 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A1" );
            command.Parameters[0].Value.Should().Be( "foo" );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "A2" );
            command.Parameters[1].Value.Should().Be( "bar" );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.String );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "A3" );
            command.Parameters[2].Value.Should().Be( "qux" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIgnoredNullableRefTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<string?[]>( new[] { "foo", null, "qux" } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A1" );
            command.Parameters[0].Value.Should().Be( "foo" );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeFalse();
            command.Parameters[1].ParameterName.Should().Be( "A2" );
            command.Parameters[1].Value.Should().Be( "qux" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIncludedNullableRefTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?[]>>(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<string?[]>( new[] { "foo", null, "qux" } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 3 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A1" );
            command.Parameters[0].Value.Should().Be( "foo" );
            command.Parameters[1].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[1].DbType.Should().Be( DbType.String );
            command.Parameters[1].IsNullable.Should().BeTrue();
            command.Parameters[1].ParameterName.Should().Be( "A2" );
            command.Parameters[1].Value.Should().BeSameAs( DBNull.Value );
            command.Parameters[2].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[2].DbType.Should().Be( DbType.String );
            command.Parameters[2].IsNullable.Should().BeFalse();
            command.Parameters[2].ParameterName.Should().Be( "A3" );
            command.Parameters[2].Value.Should().Be( "qux" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsIgnoredNutNullEmptyCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int[]>( Array.Empty<int>() ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsIgnoredNullableCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]?>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int[]?>( null ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsIncludedNullableCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]?>>(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<int[]?>( null ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsOfStringTypeWithReducedCollections()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<string>( "foo" ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.String );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsOfByteArrayTypeWithReducedCollections()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<byte[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<byte[]>( new byte[] { 1, 2, 3 } ) );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Binary );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().BeEquivalentTo( new byte[] { 1, 2, 3 } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WithAllPreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>();

        parameterBinder.Bind( command, new Source() );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WithSomePreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        var p1 = command.CreateParameter();
        command.Parameters.Add( p1 );
        command.Parameters.Add( command.CreateParameter() );
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>();

        parameterBinder.Bind( command, new Source { A = 10 } );

        using ( new AssertionScope() )
        {
            parameterBinder.Dialect.Should().BeSameAs( sut.Dialect );
            command.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Should().BeSameAs( p1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeIsAbstract()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression<IEnumerable>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeIsGenericDefinition()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( IEnumerable<> ) ) );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeIsNullableValue()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( int? ) ) );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeHasNoMembers()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression<object>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNoValidMemberForSourceTypeIsFound()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of(
            () => sut.CreateExpression<Source>( SqlParameterBinderCreationOptions.Default.SetSourceTypeMemberPredicate( _ => false ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenParameterAppearsMoreThanOnce()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default.With( SqlParameterConfiguration.From( "B", "C" ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterIsMissing()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "A", isNullable: true ) );
        interpreter.Visit( SqlNode.Parameter( "X" ) );

        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default
                    .SetContext( interpreter.Context )
                    .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", false ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenContextDoesNotExpectExistingParameter()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "A", isNullable: true ) );

        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default
                    .SetContext( interpreter.Context )
                    .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", false ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterExpectsDifferentType()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<string>( "A", isNullable: true ) );

        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default
                    .SetContext( interpreter.Context )
                    .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", false ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterShouldNotBeNullable()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<string>( "B", isNullable: false ) );

        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default
                    .SetContext( interpreter.Context )
                    .With( SqlParameterConfiguration.IgnoreMember( "A" ) )
                    .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "B", false ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterIsNullableAndNullValueIsIgnored()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "A", isNullable: true ) );

        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default
                    .SetContext( interpreter.Context )
                    .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                    .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenReducibleCollectionParameterIsPositional()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();

        var action = Lambda.Of(
            () => sut.CreateExpression<GenericSource<int[]>>(
                SqlParameterBinderCreationOptions.Default
                    .EnableCollectionReduction()
                    .With( SqlParameterConfiguration.Positional( "A", 0 ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenParameterWithIgnoredNullValuesIsPositional()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();

        var action = Lambda.Of(
            () => sut.CreateExpression<GenericSource<string>>(
                SqlParameterBinderCreationOptions.Default.With(
                    SqlParameterConfiguration.IgnoreMemberWhenNull( "A", parameterIndex: 0 ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNullablePositionalParameterIndexesAreInvalid()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();

        var action = Lambda.Of(
            () => sut.CreateExpression<Source>(
                SqlParameterBinderCreationOptions.Default
                    .With( SqlParameterConfiguration.Positional( "A", 2 ) )
                    .With( SqlParameterConfiguration.Positional( "B", 5 ) )
                    .With( SqlParameterConfiguration.Positional( "C", 2 ) )
                    .With( SqlParameterConfiguration.Positional( "D", 0 ) ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    public sealed class Source
    {
        public int? A { get; init; }
        public string? B { get; init; }
        public double? C { get; init; }
        public bool? D { get; init; }
    }

    public sealed class SourceWithFields
    {
        public int? A;
        public string? B;
    }

    public sealed class SourceWithWriteOnlyProperty
    {
        private string _b = string.Empty;

        public int? A { get; init; }

        public string B
        {
            set => _b = value;
        }
    }

    public sealed class GenericSource<T>
    {
        public GenericSource(T a)
        {
            A = a;
        }

        public T A { get; }
    }

    [Pure]
    private static SqlParameter GetParameter(string name, object? value)
    {
        return SqlParameter.Named( name, value );
    }
}
