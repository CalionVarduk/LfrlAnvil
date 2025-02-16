using System.Collections.Generic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseFactoryProviderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyProvider()
    {
        var sut = new SqlDatabaseFactoryProvider();
        sut.SupportedDialects.TestEmpty().Go();
    }

    [Fact]
    public void RegisterFactory_ShouldAddFactoryForDialect_WhenOneDoesNotExist()
    {
        var dialect = new SqlDialect( "FooSql" );
        var factory = Substitute.For<ISqlDatabaseFactory>();
        factory.Dialect.Returns( dialect );

        var sut = new SqlDatabaseFactoryProvider();

        var result = sut.RegisterFactory( factory );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.SupportedDialects.TestSequence( [ dialect ] ),
                result.GetFor( dialect ).TestRefEquals( factory ) )
            .Go();
    }

    [Fact]
    public void RegisterFactory_ShouldAddFactoryForDialect_WhenOneDoesNotExistAndProviderIsNotEmpty()
    {
        var otherDialect = new SqlDialect( "BarSql" );
        var otherFactory = Substitute.For<ISqlDatabaseFactory>();
        otherFactory.Dialect.Returns( otherDialect );

        var dialect = new SqlDialect( "FooSql" );
        var factory = Substitute.For<ISqlDatabaseFactory>();
        factory.Dialect.Returns( dialect );

        var sut = new SqlDatabaseFactoryProvider();
        sut.RegisterFactory( otherFactory );

        var result = sut.RegisterFactory( factory );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.SupportedDialects.Count.TestEquals( 2 ),
                result.SupportedDialects.TestSetEqual( [ otherDialect, dialect ] ),
                result.GetFor( dialect ).TestRefEquals( factory ),
                result.GetFor( otherDialect ).TestRefEquals( otherFactory ) )
            .Go();
    }

    [Fact]
    public void RegisterFactory_ShouldOverrideFactoryForDialect_WhenOneExists()
    {
        var dialect = new SqlDialect( "FooSql" );
        var oldFactory = Substitute.For<ISqlDatabaseFactory>();
        oldFactory.Dialect.Returns( dialect );

        var factory = Substitute.For<ISqlDatabaseFactory>();
        factory.Dialect.Returns( dialect );

        var sut = new SqlDatabaseFactoryProvider();
        sut.RegisterFactory( oldFactory );

        var result = sut.RegisterFactory( factory );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.SupportedDialects.TestSequence( [ dialect ] ),
                result.GetFor( dialect ).TestRefEquals( factory ) )
            .Go();
    }

    [Fact]
    public void GetFor_ShouldThrowKeyNotFoundException_WhenDialectIsNotRegistered()
    {
        var dialect = new SqlDialect( "FooSql" );
        var sut = new SqlDatabaseFactoryProvider();

        var action = Lambda.Of( () => sut.GetFor( dialect ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }
}
