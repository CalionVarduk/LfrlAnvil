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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.GetAll().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WhenSourceIsNotEmpty()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", 1 ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithAllowedNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Object ),
                command.Parameters[0].IsNullable.TestTrue(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestRefEquals( DBNull.Value ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithIgnoredNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 3 ),
                command.Parameters[0].TestRefEquals( p1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( 1 ),
                command.Parameters[1].TestRefEquals( p2 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "b" ),
                command.Parameters[1].Value.TestEquals( "foo" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Double ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "c" ),
                command.Parameters[2].Value.TestEquals( 5.0 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 3 ),
                command.Parameters[0].TestRefEquals( p1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestNull(),
                command.Parameters[0].Value.TestEquals( 1 ),
                command.Parameters[1].TestRefEquals( p2 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestNull(),
                command.Parameters[1].Value.TestEquals( "foo" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Double ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestNull(),
                command.Parameters[2].Value.TestEquals( 5.0 ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithAllPreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create();

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].TestRefEquals( p1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceIsEmpty()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, Enumerable.Empty<SqlParameter>() );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceIsNotEmpty()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", 1 ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WithAllowedNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Object ),
                command.Parameters[0].IsNullable.TestTrue(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestRefEquals( DBNull.Value ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WithIgnoredNullValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsStringValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", "foo" ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsByteArrayValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new byte[] { 0, 1, 2 } ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Cast<DbParameterMock>()
                    .TestSequence(
                    [
                        (p, _) => Assertion.All(
                            p.Direction.TestEquals( ParameterDirection.Input ),
                            p.DbType.TestEquals( DbType.Binary ),
                            p.IsNullable.TestFalse(),
                            p.ParameterName.TestEquals( "a" ),
                            p.Value.TestType().AssignableTo<byte[]>( value => value.TestSetEqual( new byte[] { 0, 1, 2 } ) ) )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsEmptyReducibleCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", Array.Empty<int>() ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsNonEmptyReducibleCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new[] { "foo", "bar" } ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a1" ),
                command.Parameters[0].Value.TestEquals( "foo" ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "a2" ),
                command.Parameters[1].Value.TestEquals( "bar" ) )
            .Go();
    }

    [Fact]
    public void
        Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollections_WhenSourceContainsReducibleCollectionWithIgnoredNullElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", new[] { null, "foo" } ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a1" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Object ),
                command.Parameters[0].IsNullable.TestTrue(),
                command.Parameters[0].ParameterName.TestEquals( "a1" ),
                command.Parameters[0].Value.TestRefEquals( DBNull.Value ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "a2" ),
                command.Parameters[1].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateCorrectParameterBinder_WithReducedCollectionsAndAllPreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new[] { GetParameter( "a", null ) } );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].TestRefEquals( p1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceIsNotProvided()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>();

        parameterBinder.Bind( command );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 4 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "B" ),
                command.Parameters[1].Value.TestEquals( "foo" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Double ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "C" ),
                command.Parameters[2].Value.TestEquals( 5.0 ),
                command.Parameters[3].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[3].DbType.TestEquals( DbType.Boolean ),
                command.Parameters[3].IsNullable.TestFalse(),
                command.Parameters[3].ParameterName.TestEquals( "D" ),
                command.Parameters[3].Value.TestEquals( true ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 4 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Double ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestNull(),
                command.Parameters[0].Value.TestEquals( 5.0 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.Boolean ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestNull(),
                command.Parameters[1].Value.TestEquals( true ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestNull(),
                command.Parameters[2].Value.TestEquals( 10 ),
                command.Parameters[3].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[3].DbType.TestEquals( DbType.String ),
                command.Parameters[3].IsNullable.TestFalse(),
                command.Parameters[3].ParameterName.TestNull(),
                command.Parameters[3].Value.TestEquals( "foo" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 4 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Double ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestNull(),
                command.Parameters[0].Value.TestEquals( 5.0 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestNull(),
                command.Parameters[1].Value.TestEquals( "foo" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "A" ),
                command.Parameters[2].Value.TestEquals( 10 ),
                command.Parameters[3].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[3].DbType.TestEquals( DbType.Boolean ),
                command.Parameters[3].IsNullable.TestFalse(),
                command.Parameters[3].ParameterName.TestEquals( "D" ),
                command.Parameters[3].Value.TestEquals( true ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 4 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "B" ),
                command.Parameters[1].Value.TestEquals( "foo" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Double ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "C" ),
                command.Parameters[2].Value.TestEquals( 5.0 ),
                command.Parameters[3].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[3].DbType.TestEquals( DbType.Boolean ),
                command.Parameters[3].IsNullable.TestFalse(),
                command.Parameters[3].ParameterName.TestEquals( "D" ),
                command.Parameters[3].Value.TestEquals( true ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "B" ),
                command.Parameters[1].Value.TestEquals( "foo" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "B" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "E" ),
                command.Parameters[1].Value.TestEquals( "foo" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "E" ),
                command.Parameters[1].Value.TestEquals( "fooTrue" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "E" ),
                command.Parameters[1].Value.TestEquals( 50 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( "fooFalse" ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 4 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "B" ),
                command.Parameters[1].Value.TestEquals( "foo" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Double ),
                command.Parameters[2].IsNullable.TestTrue(),
                command.Parameters[2].ParameterName.TestEquals( "C" ),
                command.Parameters[2].Value.TestRefEquals( DBNull.Value ),
                command.Parameters[3].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[3].DbType.TestEquals( DbType.Boolean ),
                command.Parameters[3].IsNullable.TestFalse(),
                command.Parameters[3].ParameterName.TestEquals( "D" ),
                command.Parameters[3].Value.TestEquals( true ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestNull(),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestNull(),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNotNullRefType()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string>>();

        parameterBinder.Bind( command, new GenericSource<string>( "foo" ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNotNullValueType()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int>>();

        parameterBinder.Bind( command, new GenericSource<int>( 10 ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableRefTypeWithValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?>>();

        parameterBinder.Bind( command, new GenericSource<string?>( "foo" ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableRefTypeWithIgnoredNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?>>();

        parameterBinder.Bind( command, new GenericSource<string?>( null ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableRefTypeWithIncludedNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?>>(
            SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<string?>( null ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestTrue(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestRefEquals( DBNull.Value ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableValueTypeWithValue()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?>>();

        parameterBinder.Bind( command, new GenericSource<int?>( 10 ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableValueTypeWithIgnoredNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?>>();

        parameterBinder.Bind( command, new GenericSource<int?>( null ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsScalarNullableValueTypeWithIncludedNull()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?>>(
            SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<int?>( null ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestTrue(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestRefEquals( DBNull.Value ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithNotNullValueTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int[]>( new[] { 10, 20, 30 } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 3 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A1" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "A2" ),
                command.Parameters[1].Value.TestEquals( 20 ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "A3" ),
                command.Parameters[2].Value.TestEquals( 30 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIgnoredNullableValueTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int?[]>( new int?[] { 10, null, 30 } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A1" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "A2" ),
                command.Parameters[1].Value.TestEquals( 30 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIncludedNullableValueTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int?[]>>(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<int?[]>( new int?[] { 10, null, 30 } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 3 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A1" ),
                command.Parameters[0].Value.TestEquals( 10 ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[1].IsNullable.TestTrue(),
                command.Parameters[1].ParameterName.TestEquals( "A2" ),
                command.Parameters[1].Value.TestRefEquals( DBNull.Value ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "A3" ),
                command.Parameters[2].Value.TestEquals( 30 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithNotNullRefTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<string[]>( new[] { "foo", "bar", "qux" } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 3 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A1" ),
                command.Parameters[0].Value.TestEquals( "foo" ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "A2" ),
                command.Parameters[1].Value.TestEquals( "bar" ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.String ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "A3" ),
                command.Parameters[2].Value.TestEquals( "qux" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIgnoredNullableRefTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<string?[]>( new[] { "foo", null, "qux" } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 2 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A1" ),
                command.Parameters[0].Value.TestEquals( "foo" ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestFalse(),
                command.Parameters[1].ParameterName.TestEquals( "A2" ),
                command.Parameters[1].Value.TestEquals( "qux" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsNotNullCollectionWithIncludedNullableRefTypeElements()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string?[]>>(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<string?[]>( new[] { "foo", null, "qux" } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 3 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A1" ),
                command.Parameters[0].Value.TestEquals( "foo" ),
                command.Parameters[1].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[1].DbType.TestEquals( DbType.String ),
                command.Parameters[1].IsNullable.TestTrue(),
                command.Parameters[1].ParameterName.TestEquals( "A2" ),
                command.Parameters[1].Value.TestRefEquals( DBNull.Value ),
                command.Parameters[2].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[2].DbType.TestEquals( DbType.String ),
                command.Parameters[2].IsNullable.TestFalse(),
                command.Parameters[2].ParameterName.TestEquals( "A3" ),
                command.Parameters[2].Value.TestEquals( "qux" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsIgnoredNutNullEmptyCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int[]>( Array.Empty<int>() ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsIgnoredNullableCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]?>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<int[]?>( null ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsIncludedNullableCollection()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<int[]?>>(
            SqlParameterBinderCreationOptions.Default.EnableCollectionReduction().EnableIgnoringOfNullValues( false ) );

        parameterBinder.Bind( command, new GenericSource<int[]?>( null ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsOfStringTypeWithReducedCollections()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<string>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<string>( "foo" ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.String ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WhenSourceMemberIsOfByteArrayTypeWithReducedCollections()
    {
        var command = new DbCommandMock();
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<GenericSource<byte[]>>( SqlParameterBinderCreationOptions.Default.EnableCollectionReduction() );

        parameterBinder.Bind( command, new GenericSource<byte[]>( new byte[] { 1, 2, 3 } ) );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Cast<DbParameterMock>()
                    .TestSequence(
                    [
                        (p, _) => Assertion.All(
                            p.Direction.TestEquals( ParameterDirection.Input ),
                            p.DbType.TestEquals( DbType.Binary ),
                            p.IsNullable.TestFalse(),
                            p.ParameterName.TestEquals( "A" ),
                            p.Value.TestType().AssignableTo<byte[]>( value => value.TestSetEqual( new byte[] { 1, 2, 3 } ) ) )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateCorrectParameterBinder_WithAllPreExistingParametersBeingExcess()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( command.CreateParameter() );

        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var parameterBinder = sut.Create<Source>();

        parameterBinder.Bind( command, new Source() );

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                parameterBinder.Dialect.TestRefEquals( sut.Dialect ),
                command.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].TestRefEquals( p1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 10 ) )
            .Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeIsAbstract()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression<IEnumerable>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeIsGenericDefinition()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( IEnumerable<> ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeIsNullableValue()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( int? ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenSourceTypeHasNoMembers()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression<object>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNoValidMemberForSourceTypeIsFound()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () =>
            sut.CreateExpression<Source>( SqlParameterBinderCreationOptions.Default.SetSourceTypeMemberPredicate( _ => false ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenParameterAppearsMoreThanOnce()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var action = Lambda.Of( () =>
            sut.CreateExpression<Source>( SqlParameterBinderCreationOptions.Default.With( SqlParameterConfiguration.From( "B", "C" ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterIsMissing()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "A", isNullable: true ) );
        interpreter.Visit( SqlNode.Parameter( "X" ) );

        var action = Lambda.Of( () => sut.CreateExpression<Source>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", false ) )
                .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenContextDoesNotExpectExistingParameter()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "A", isNullable: true ) );

        var action = Lambda.Of( () => sut.CreateExpression<Source>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", false ) )
                .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterExpectsDifferentType()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<string>( "A", isNullable: true ) );

        var action = Lambda.Of( () => sut.CreateExpression<Source>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", false ) )
                .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterShouldNotBeNullable()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<string>( "B", isNullable: false ) );

        var action = Lambda.Of( () => sut.CreateExpression<Source>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.IgnoreMember( "A" ) )
                .With( SqlParameterConfiguration.IgnoreMemberWhenNull( "B", false ) )
                .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenExpectedContextParameterIsNullableAndNullValueIsIgnored()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();
        var interpreter = new SqlNodeDebugInterpreter();
        interpreter.Visit( SqlNode.Parameter<int>( "A", isNullable: true ) );

        var action = Lambda.Of( () => sut.CreateExpression<Source>(
            SqlParameterBinderCreationOptions.Default
                .SetContext( interpreter.Context )
                .With( SqlParameterConfiguration.IgnoreMember( "B" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "C" ) )
                .With( SqlParameterConfiguration.IgnoreMember( "D" ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenReducibleCollectionParameterIsPositional()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();

        var action = Lambda.Of( () => sut.CreateExpression<GenericSource<int[]>>(
            SqlParameterBinderCreationOptions.Default
                .EnableCollectionReduction()
                .With( SqlParameterConfiguration.Positional( "A", 0 ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenParameterWithIgnoredNullValuesIsPositional()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();

        var action = Lambda.Of( () => sut.CreateExpression<GenericSource<string>>(
            SqlParameterBinderCreationOptions.Default.With( SqlParameterConfiguration.IgnoreMemberWhenNull( "A", parameterIndex: 0 ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNullablePositionalParameterIndexesAreInvalid()
    {
        var sut = SqlParameterBinderFactoryMock.CreateInstance();

        var action = Lambda.Of( () => sut.CreateExpression<Source>(
            SqlParameterBinderCreationOptions.Default
                .With( SqlParameterConfiguration.Positional( "A", 2 ) )
                .With( SqlParameterConfiguration.Positional( "B", 5 ) )
                .With( SqlParameterConfiguration.Positional( "C", 2 ) )
                .With( SqlParameterConfiguration.Positional( "D", 0 ) ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
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
